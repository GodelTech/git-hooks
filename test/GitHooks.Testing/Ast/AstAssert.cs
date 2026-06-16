using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Testing.Ast;

public static class AstAssert
{
    public static StringKeyFieldNode<ExpressionNode> HasIntegerField(
        StringKeyFieldNode<ExpressionNode> field,
        string key,
        int value)
    {
        Assert.NotNull(field);

        Assert.Equal(key, field.Key);

        HasIntegerValue(field.Value, value);

        return field;
    }

    public static StringKeyFieldNode<ExpressionNode> HasStringField(
        StringKeyFieldNode<ExpressionNode> field,
        string key,
        string value)
    {
        Assert.NotNull(field);

        Assert.Equal(key, field.Key);

        HasStringValue(field.Value, value);

        return field;
    }

    private static void HasIntegerValue(
        ExpressionNode expression,
        int expected)
    {
        Assert.NotNull(expression);

        var literal = Assert.IsType<IntegerLiteralExpressionNode>(expression);

        Assert.Equal(
            expected,
            literal.Value);
    }

    private static void HasStringValue(
        ExpressionNode expression,
        string expected)
    {
        Assert.NotNull(expression);

        var literal = Assert.IsType<StringLiteralExpressionNode>(expression);

        Assert.Equal(
            expected,
            literal.Value);
    }
}
