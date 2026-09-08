using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;

namespace GitHooks.Domain.Tests.Ast.Mappings.Parameters;

public sealed class ParameterNodeExtensionsTests
{
    [Fact]
    public void TryGetName_NullParameter_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => ParameterNodeExtensions.TryGetName(
                null!,
                out _));

        Assert.Equal(
            "node",
            exception.ParamName);
    }

    [Fact]
    public void TryGetName_StringLiteralName_ReturnsTrue()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        var result = parameter.TryGetName(
            out var name);

        Assert.True(result);

        Assert.Equal(
            "configuration",
            name);
    }

    [Fact]
    public void TryGetName_EmptyName_ReturnsFalse()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName(string.Empty)
            .Build();

        var result = parameter.TryGetName(
            out var name);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            name);
    }

    [Fact]
    public void TryGetName_WhitespaceName_ReturnsFalse()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("   ")
            .Build();

        var result = parameter.TryGetName(
            out var name);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            name);
    }

    [Fact]
    public void TryGetName_IntegerLiteralName_ReturnsTrue()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithIntegerValue(v => v.WithValue(42)))
            .Build();

        var result = parameter.TryGetName(
            out var name);

        Assert.True(result);

        Assert.Equal(
            "42",
            name);
    }

    [Fact]
    public void TryGetName_ParameterVariableExpression_ReturnsFalse()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithInterpolatedStringValue(v => v
                    .WithParameterVariablePart("configuration")))
            .Build();

        var result = parameter.TryGetName(
            out var name);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            name);
    }

    [Fact]
    public void TryGetDefaultValue_NullParameter_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => ParameterNodeExtensions.TryGetDefaultValue(
                null!,
                out _));

        Assert.Equal(
            "node",
            exception.ParamName);
    }

    [Fact]
    public void TryGetDefaultValue_StringLiteralDefault_ReturnsTrue()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("serverName")
            .WithDefaultValue("localhost")
            .Build();

        var result = parameter.TryGetDefaultValue(
            out var defaultValue);

        Assert.True(result);

        Assert.Equal(
            "localhost",
            defaultValue);
    }

    [Fact]
    public void TryGetDefaultValue_MissingDefaultValue_ReturnsFalse()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("serverName")
            .Build();

        var result = parameter.TryGetDefaultValue(
            out var defaultValue);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            defaultValue);
    }

    [Fact]
    public void TryGetDefaultValue_IntegerLiteralDefault_ReturnsTrue()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("serverName")
            .WithDefaultValue(x => x
                .WithKey("defaultValue")
                .WithIntegerValue(v => v.WithValue(42)))
            .Build();

        var result = parameter.TryGetDefaultValue(
            out var defaultValue);

        Assert.True(result);

        Assert.Equal(
            "42",
            defaultValue);
    }

    [Fact]
    public void TryGetDefaultValue_ParameterVariableExpression_ReturnsFalse()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("serverName")
            .WithDefaultValue(x => x
                .WithKey("defaultValue")
                .WithInterpolatedStringValue(v => v
                    .WithParameterVariablePart("configuration")))
            .Build();

        var result = parameter.TryGetDefaultValue(
            out var defaultValue);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            defaultValue);
    }
}
