using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Steps;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

internal sealed class StepParser(IEnumerable<IStepFieldHandler> handlers)
{
    private readonly Dictionary<string, IStepFieldHandler> _handlers
        = handlers.ToDictionary(h => h.Key, StringComparer.Ordinal);

    public StepNode Parse(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        var fields = new StepFields();

        while (!reader.Is<MappingEnd>())
        {
            if (!reader.Is<Scalar>())
            {
                var keyNode = reader.ReadUnknownNode();
                var unknownFields = fields.UnknownFields.ToList();
                unknownFields.Add(reader.ReadUnknownField(keyNode));
                fields = fields with
                {
                    UnknownFields = unknownFields
                };
                continue;
            }

            var key = reader.Read<Scalar>();

            if (_handlers.TryGetValue(key.Value, out var handler))
            {
                fields = handler.Apply(reader, fields);
            }
            else
            {
                var unknownFields = fields.UnknownFields.ToList();
                unknownFields.Add(reader.ReadUnknownField(key));
                fields = fields with
                {
                    UnknownFields = unknownFields
                };
            }
        }

        var end = reader.Read<MappingEnd>();

        var span = reader.SpanOf(start, end);

        return BuildStep(fields, span);
    }

    private static StepNode BuildStep(StepFields fields, SourceSpan span)
    {
        StepNode node = (fields.Script, fields.Template) switch
        {
            // Only script is defined.
            ({ } script, null) => new ScriptStepNode(script, span),

            // Only template is defined.
            (null, { } template) => new TemplateStepNode(template, fields.Parameters, span),

            // No supported step type was provided.
            (null, null) => throw new YamlParseException(
                "Step must contain 'script' or 'template'",
                span
            ),

            // Script and template are mutually exclusive.
            ({ }, { }) => throw new YamlParseException(
                "Step can contain only one of 'script' or 'template'",
                span
            )
        };

        return node with
        {
            DisplayName = fields.DisplayName,
            Condition = fields.Condition,
            TimeoutInMinutes = fields.TimeoutInMinutes,
            WorkingDirectory = fields.WorkingDirectory,
            Env = fields.Env,
            UnknownFields = fields.UnknownFields
        };
    }
}
