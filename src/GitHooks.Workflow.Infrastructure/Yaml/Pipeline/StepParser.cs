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
            var key = reader.Read<Scalar>().Value;

            if (_handlers.TryGetValue(key, out var handler))
            {
                fields = handler.Apply(reader, fields);
            }
            else
            {
                reader.SkipNode();
            }
        }

        var end = reader.Read<MappingEnd>();

        var span = reader.SpanOf(start, end);

        return BuildStep(fields, span);
    }

    private static StepNode BuildStep(StepFields fields, SourceSpan span)
    {
        var typeCount =
            (fields.Script is not null ? 1 : 0) +
            (fields.Template is not null ? 1 : 0);

        if (typeCount == 0)
        {
            throw new YamlParseException(
                "Step must contain 'script' or 'template'",
                span
            );
        }

        if (typeCount > 1)
        {
            throw new YamlParseException(
                "Step can contain only one of 'script' or 'template'",
                span
            );
        }

        StepNode node = fields.Script is not null
            ? new ScriptStepNode(fields.Script, span)
            : fields.Template is not null
                ? new TemplateStepNode(fields.Template, fields.Parameters, span)
                : throw new YamlParseException(
                    "Step must contain 'script' or 'template'",
                    span
                );

        return node with
        {
            DisplayName = fields.DisplayName,
            Condition = fields.Condition,
            TimeoutInMinutes = fields.TimeoutInMinutes,
            WorkingDirectory = fields.WorkingDirectory,
            Env = fields.Env
        };
    }
}
