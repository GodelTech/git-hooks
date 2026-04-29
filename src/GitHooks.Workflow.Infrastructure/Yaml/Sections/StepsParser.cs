using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Sections;

internal sealed class StepsParser(StepParser stepParser)
{
    private readonly StepParser _stepParser = stepParser;

    public List<StepNode> Parse(YamlReader reader)
    {
        _ = reader.Read<SequenceStart>();

        var steps = new List<StepNode>();

        while (!reader.Is<SequenceEnd>())
        {
            var step = _stepParser.Parse(reader);
            steps.Add(step);
        }

        _ = reader.Read<SequenceEnd>();

        // Span of sequence itself is not returned here,
        // Steps are individual nodes with spans.
        return steps;
    }
}
