using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

internal sealed class PipelineRootParser(StepsParser stepsParser)
{
    private readonly StepsParser _stepsParser = stepsParser;

    public PipelineNode Parse(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        IReadOnlyList<StepNode>? steps = null;

        while (!reader.Is<MappingEnd>())
        {
            var key = reader.Read<Scalar>().Value;

            if (key == "steps")
            {
                steps = _stepsParser.Parse(reader);
            }
            else
            {
                reader.SkipNode();
            }
        }

        var end = reader.Read<MappingEnd>();

        var span = reader.SpanOf(start, end);

        return steps is null
            ? throw new YamlParseException("Pipeline must contain 'steps'", span)
            : new PipelineNode(steps, span);
    }
}
