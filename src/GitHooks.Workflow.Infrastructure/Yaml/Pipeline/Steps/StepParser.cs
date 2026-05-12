using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

internal sealed class StepParser(IEnumerable<IStepFieldHandler> handlers, IEnumerable<IStepNodeBuilder> builders)
{
    private readonly Dictionary<string, IStepFieldHandler> _handlers = handlers.ToDictionary(h => h.Key, StringComparer.Ordinal);
    private readonly List<IStepNodeBuilder> _builders = [.. builders];

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

    private StepNode BuildStep(StepFields fields, SourceSpan span)
    {
        var applicableBuilders = _builders
            .Where(builder => builder.CanBuild(fields))
            .ToList();

        return applicableBuilders.Count switch
        {
            1 => applicableBuilders[0].Build(fields, span),
            0 => throw new YamlParseException(
                "Step must contain 'script' or 'template'",
                span
            ),
            _ => throw new YamlParseException(
                "Step can contain only one of 'script' or 'template'",
                span
            )
        };
    }
}
