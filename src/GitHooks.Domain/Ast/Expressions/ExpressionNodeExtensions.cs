using System.Globalization;

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

        if (expression is BooleanLiteralExpressionNode booleanLiteral)
        {
            value = booleanLiteral.Value.ToString();
            return true;
        }

        if (expression is IntegerLiteralExpressionNode integerLiteral)
        {
            value = integerLiteral.Value.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        value = string.Empty;
        return false;
    }

    public static bool TryGetStringLiteralValue(
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
