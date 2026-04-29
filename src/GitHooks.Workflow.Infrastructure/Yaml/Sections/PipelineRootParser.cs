using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Sections;

internal sealed class PipelineRootParser(StepsParser stepsParser)
{
    private readonly StepsParser _stepsParser = stepsParser;

    public PipelineNode Parse(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        var steps = new List<StepNode>();

        while (!reader.Is<MappingEnd>())
        {
            var key = reader.Read<Scalar>().Value;

            switch (key)
            {
                case "steps":
                    steps = _stepsParser.Parse(reader);
                    break;

                default:
                    reader.SkipNode();
                    break;
            }
        }

        var end = reader.Read<MappingEnd>();

        return new PipelineNode(
            reader.SpanOf(start, end),
            steps
        );
    }
}
