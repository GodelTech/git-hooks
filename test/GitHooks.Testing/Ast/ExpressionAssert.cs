using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class ExpressionAssert
{
    public static IntegerLiteralExpressionNode IsIntegerLiteralExpression(
        ExpressionNode expression,
        int value,
        SourceSpan span)
    {
        var actualExpression = IsValidExpression<IntegerLiteralExpressionNode>(
            expression,
            span);

        Assert.Equal(
            value,
            actualExpression.Value);

        return actualExpression;
    }

    public static StringLiteralExpressionNode IsStringLiteralExpression(
        ExpressionNode expression,
        string value,
        SourceSpan span)
    {
        var actualExpression = IsValidExpression<StringLiteralExpressionNode>(
            expression,
            span);

        Assert.Equal(
            value,
            actualExpression.Value);

        return actualExpression;
    }

    public static ParameterVariableExpressionNode IsParameterVariableExpression(
        ExpressionNode expression,
        string name,
        SourceSpan span)
    {
        var actualExpression = IsValidExpression<ParameterVariableExpressionNode>(
            expression,
            span);

        Assert.Equal(
            name,
            actualExpression.Name);

        return actualExpression;
    }

    public static InvalidVariableExpressionNode IsInvalidVariableExpression(
        ExpressionNode expression,
        string text,
        SourceSpan span)
    {
        var actualExpression = IsValidExpression<InvalidVariableExpressionNode>(
            expression,
            span);

        Assert.Equal(
            text,
            actualExpression.Text);

        return actualExpression;
    }

    private static TNode IsValidExpression<TNode>(
        ExpressionNode expression,
        SourceSpan span)
        where TNode : ExpressionNode
    {
        return AstAssert.IsValidNode<TNode>(
            expression,
            span);
    }
}
