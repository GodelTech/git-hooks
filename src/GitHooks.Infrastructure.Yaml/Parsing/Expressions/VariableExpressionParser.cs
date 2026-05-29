using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed class VariableExpressionParser
{
#pragma warning disable CA1822 // Mark members as static // Parser kept instance-based for parser graph consistency.
    public VariableExpressionNode Parse(string expression, SourceSpan span)
#pragma warning restore CA1822 // Mark members as static
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        return new VariableExpressionNode
        {
            Path = expression.Trim(),
            Span = span
        };
    }
}
