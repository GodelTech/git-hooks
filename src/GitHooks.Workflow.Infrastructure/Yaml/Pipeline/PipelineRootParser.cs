using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

internal sealed class PipelineRootParser(StepsParser stepsParser)
{
    private readonly StepsParser _stepsParser = stepsParser;

    public PipelineNode Parse(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        IReadOnlyList<StepNode>? steps = null;
        var unknownFields = new List<UnknownFieldNode>();

        while (!reader.Is<MappingEnd>())
        {
            if (!reader.Is<Scalar>())
            {
                var keyNode = reader.ReadUnknownNode();
                unknownFields.Add(reader.ReadUnknownField(keyNode));
                continue;
            }

            var key = reader.Read<Scalar>();

            if (key.Value == "steps")
            {
                steps = _stepsParser.Parse(reader);
            }
            else
            {
                unknownFields.Add(reader.ReadUnknownField(key));
            }
        }

        var end = reader.Read<MappingEnd>();

        var span = reader.SpanOf(start, end);

        return steps is null
            ? throw new YamlParseException("Pipeline must contain 'steps'", span)
            : new PipelineNode(steps, span)
            {
                UnknownFields = unknownFields
            };
    }
}
