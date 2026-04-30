using GitHooks.Workflow.Application.Ast.Expressions;

namespace GitHooks.Workflow.Infrastructure.Yaml.Expressions;

internal sealed class InterpolationParser
{
#pragma warning disable CA1822 // Mark members as static
    public InterpolatedStringNode Parse(string value)
#pragma warning restore CA1822 // Mark members as static
    {
        return new(value);
    }
}
