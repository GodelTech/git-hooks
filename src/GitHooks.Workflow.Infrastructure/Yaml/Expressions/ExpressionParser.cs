using GitHooks.Workflow.Application.Ast.Expressions;

namespace GitHooks.Workflow.Infrastructure.Yaml.Expressions;

internal sealed class ExpressionParser
{
#pragma warning disable CA1822 // Mark members as static
    public ExpressionNode Parse(string value)
#pragma warning restore CA1822 // Mark members as static
    {
        return new RawExpressionNode(value.Trim());
    }
}

