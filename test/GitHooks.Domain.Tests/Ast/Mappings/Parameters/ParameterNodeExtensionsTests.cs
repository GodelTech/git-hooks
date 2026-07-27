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
    public void TryGetName_NonStringExpression_ReturnsFalse()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithIntegerValue(v => v.WithValue(42)))
            .Build();

        var result = parameter.TryGetName(
            out var name);

        Assert.False(result);

        Assert.Equal(
            string.Empty,
            name);
    }
}
