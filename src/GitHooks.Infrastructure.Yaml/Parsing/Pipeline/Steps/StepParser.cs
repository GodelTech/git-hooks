using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepParser
{
#pragma warning disable CA1822 // Mark members as static
    public StepNode Parse(YamlParserCursor cursor)
#pragma warning restore CA1822 // Mark members as static
    {
        ArgumentNullException.ThrowIfNull(cursor);

        throw new NotImplementedException("Step parsing is not implemented yet.");
    }
}
