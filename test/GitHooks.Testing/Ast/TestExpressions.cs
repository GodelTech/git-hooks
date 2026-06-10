using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestExpressions
{
    public static BooleanLiteralExpressionNode Boolean(
        bool value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static IntegerLiteralExpressionNode Integer(
        int value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static StringLiteralExpressionNode String(
        string value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static InterpolatedStringExpressionNode InterpolatedString(
        params ExpressionNode[] parts)
    {
        return new()
        {
            Parts = parts,
            Span = SourceSpan.Unknown
        };
    }

    public static VariableExpressionNode Variable(
        string path)
    {
        return new()
        {
            Path = path,
            Span = SourceSpan.Unknown
        };
    }
}
