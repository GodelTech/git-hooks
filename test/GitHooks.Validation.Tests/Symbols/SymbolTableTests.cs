using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Validation.Symbols;

namespace GitHooks.Validation.Tests.Symbols;

public sealed class SymbolTableTests
{
    [Fact]
    public void AddParameter_NullParameter_Throws()
    {
        var symbols = new SymbolTable();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => symbols.AddParameter(null!));

        Assert.Equal(
            "parameter",
            exception.ParamName);
    }

    [Fact]
    public void AddParameter_ParameterWithoutName_DoesNotAddParameter()
    {
        var symbols = new SymbolTable();

        var parameter = new ParameterNodeBuilder()
            .WithName(string.Empty)
            .Build();

        symbols.AddParameter(parameter);

        Assert.False(
            symbols.ContainsParameter(
                "configuration"));
    }

    [Fact]
    public void AddParameter_ParameterWithName_AddsParameter()
    {
        var symbols = new SymbolTable();

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        symbols.AddParameter(parameter);

        Assert.True(
            symbols.TryGetParameter(
                "configuration",
                out var actual));

        Assert.Same(
            parameter,
            actual);
    }

    [Fact]
    public void AddParameter_DuplicateParameter_KeepsFirstParameter()
    {
        var symbols = new SymbolTable();

        var first = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        var second = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        symbols.AddParameter(first);
        symbols.AddParameter(second);

        Assert.True(
            symbols.TryGetParameter(
                "configuration",
                out var actual));

        Assert.Same(
            first,
            actual);
    }

    [Fact]
    public void ContainsParameter_NullName_Throws()
    {
        var symbols = new SymbolTable();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => symbols.ContainsParameter(null!));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void ContainsParameter_EmptyName_Throws(
        string name)
    {
        var symbols = new SymbolTable();

        var exception =
            Assert.Throws<ArgumentException>(
                () => symbols.ContainsParameter(name));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    [Fact]
    public void ContainsParameter_UnknownParameter_ReturnsFalse()
    {
        var symbols = new SymbolTable();

        var result = symbols.ContainsParameter(
            "configuration");

        Assert.False(result);
    }

    [Fact]
    public void ContainsParameter_KnownParameter_ReturnsTrue()
    {
        var symbols = new SymbolTable();

        symbols.AddParameter(
            new ParameterNodeBuilder()
                .WithName("configuration")
                .Build());

        var result = symbols.ContainsParameter(
            "configuration");

        Assert.True(result);
    }

    [Fact]
    public void TryGetParameter_NullName_Throws()
    {
        var symbols = new SymbolTable();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => symbols.TryGetParameter(
                    null!,
                    out _));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void TryGetParameter_EmptyName_Throws(
        string name)
    {
        var symbols = new SymbolTable();

        var exception =
            Assert.Throws<ArgumentException>(
                () => symbols.TryGetParameter(name, out _));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    [Fact]
    public void TryGetParameter_UnknownParameter_ReturnsFalse()
    {
        var symbols = new SymbolTable();

        var result = symbols.TryGetParameter(
            "configuration",
            out var parameter);

        Assert.False(result);
        Assert.Null(parameter);
    }

    [Fact]
    public void TryGetParameter_KnownParameter_ReturnsTrue()
    {
        var symbols = new SymbolTable();

        var expected = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        symbols.AddParameter(expected);

        var result = symbols.TryGetParameter(
            "configuration",
            out var actual);

        Assert.True(result);
        Assert.Same(expected, actual);
    }

    [Fact]
    public void TryGetParameter_NameDiffersOnlyByCase_ReturnsParameter()
    {
        var symbols = new SymbolTable();

        var expected = new ParameterNodeBuilder()
            .WithName("Configuration")
            .Build();

        symbols.AddParameter(expected);

        var result = symbols.TryGetParameter(
            "configuration",
            out var actual);

        Assert.True(result);
        Assert.Same(expected, actual);
    }
}
