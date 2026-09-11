using GitHooks.Domain.Ast.Expressions;
using GitHooks.Testing.Ast.Builders.Expressions;

namespace GitHooks.Domain.Tests.Ast.Expressions;

public sealed class ExpressionNodeExtensionsTests
{
    [Fact]
    public void TryGetStringValue_NullExpression_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => ExpressionNodeExtensions.TryGetStringValue(
                null!,
                out _));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }

    [Fact]
    public void TryGetStringValue_StringLiteral_ReturnsTrue()
    {
        var expression = new StringLiteralExpressionNodeBuilder()
            .WithValue("test")
            .Build();

        var result = expression.TryGetStringValue(
            out var value);

        Assert.True(result);

        Assert.Equal(
            "test",
            value);
    }

    [Fact]
    public void TryGetStringValue_BooleanLiteral_ReturnsTrue()
    {
        var expression = new BooleanLiteralExpressionNodeBuilder()
            .WithValue(true)
            .Build();

        var result = expression.TryGetStringValue(
            out var value);

        Assert.True(result);

        Assert.Equal(
            bool.TrueString,
            value);
    }

    [Fact]
    public void TryGetStringValue_IntegerLiteral_ReturnsTrue()
    {
        var expression = new IntegerLiteralExpressionNodeBuilder()
            .WithValue(42)
            .Build();

        var result = expression.TryGetStringValue(
            out var value);

        Assert.True(result);

        Assert.Equal(
            "42",
            value);
    }

    [Fact]
    public void TryGetStringValue_ParameterVariableExpression_ReturnsFalse()
    {
        var expression = new ParameterVariableExpressionNodeBuilder()
            .WithName("configuration")
            .Build();

        var result = expression.TryGetStringValue(
            out var value);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            value);
    }

    [Fact]
    public void TryGetStringValue_InvalidVariableExpression_ReturnsFalse()
    {
        var expression = new InvalidVariableExpressionNodeBuilder()
            .WithText("foo.bar")
            .Build();

        var result = expression.TryGetStringValue(
            out var value);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            value);
    }

    [Fact]
    public void TryGetStringValue_InterpolatedStringExpression_ReturnsFalse()
    {
        var expression = new InterpolatedStringExpressionNodeBuilder()
            .WithPart("value-")
            .WithParameterVariablePart("configuration")
            .Build();

        var result = expression.TryGetStringValue(
            out var value);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            value);
    }
}
