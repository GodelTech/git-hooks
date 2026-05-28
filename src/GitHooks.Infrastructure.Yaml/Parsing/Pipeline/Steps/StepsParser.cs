using GitHooks.Domain.Ast.Mappings.Steps;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepsParser(
    StepParser stepParser)
{
    private readonly StepParser _stepParser
        = stepParser ?? throw new ArgumentNullException(nameof(stepParser));

    public IReadOnlyList<StepNode> Parse(YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        _ = cursor.Read<SequenceStart>();

        var steps = new List<StepNode>();

        while (!cursor.Is<SequenceEnd>())
        {
            var step = _stepParser.Parse(cursor);

            steps.Add(step);
        }

        _ = cursor.Read<SequenceEnd>();

        return steps;
    }
}
