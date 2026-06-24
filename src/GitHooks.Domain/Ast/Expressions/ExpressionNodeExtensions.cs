namespace GitHooks.Domain.Ast.Expressions;

public static class ExpressionNodeExtensions
{
    public static bool TryGetStringValue(
        this ExpressionNode expression,
        out string value)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression is StringLiteralExpressionNode literal)
        {
            value = literal.Value;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
