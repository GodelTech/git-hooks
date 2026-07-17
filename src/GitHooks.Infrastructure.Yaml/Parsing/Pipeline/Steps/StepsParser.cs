using GitHooks.Domain.Ast.Mappings.Steps;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepsParser(
    StepParser stepParser)
{
    private readonly StepParser _stepParser
        = stepParser ?? throw new ArgumentNullException(nameof(stepParser));

    public IReadOnlyList<StepNode> Parse(
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _ = context.Cursor.Read<SequenceStart>();

        var steps = new List<StepNode>();

        while (!context.Cursor.Is<SequenceEnd>())
        {
            var step = _stepParser.Parse(context);

            steps.Add(step);
        }

        _ = context.Cursor.Read<SequenceEnd>();

        return steps;
    }
}
