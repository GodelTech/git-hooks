using GitHooks.Compilation.Binding;

namespace GitHooks.Compilation.Tests.Binding;

public sealed class ParameterValueTableTests
{
    [Fact]
    public void TryGetValue_UnknownName_ReturnsFalse()
    {
        var table = new ParameterValueTable();

        var result = table.TryGetValue(
            "serverName",
            out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void SetValue_ThenTryGetValue_ReturnsValue()
    {
        var table = new ParameterValueTable();

        table.SetValue("serverName", "localhost");

        var result = table.TryGetValue(
            "serverName",
            out var value);

        Assert.True(result);

        Assert.Equal(
            "localhost",
            value);
    }

    [Fact]
    public void TryGetValue_IsCaseInsensitive()
    {
        var table = new ParameterValueTable();

        table.SetValue("serverName", "localhost");

        var result = table.TryGetValue(
            "SERVERNAME",
            out var value);

        Assert.True(result);

        Assert.Equal(
            "localhost",
            value);
    }

    [Fact]
    public void ContainsValue_UnknownName_ReturnsFalse()
    {
        var table = new ParameterValueTable();

        Assert.False(table.ContainsValue("serverName"));
    }

    [Fact]
    public void ContainsValue_KnownName_ReturnsTrue()
    {
        var table = new ParameterValueTable();

        table.SetValue("serverName", "localhost");

        Assert.True(table.ContainsValue("serverName"));
    }

    [Fact]
    public void SetValue_SameNameTwice_OverwritesPreviousValue()
    {
        var table = new ParameterValueTable();

        table.SetValue("serverName", "localhost");
        table.SetValue("serverName", "remotehost");

        table.TryGetValue(
            "serverName",
            out var value);

        Assert.Equal(
            "remotehost",
            value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void SetValue_InvalidName_Throws(
        string? name)
    {
        var table = new ParameterValueTable();

        Assert.ThrowsAny<ArgumentException>(
            () => table.SetValue(name!, "localhost"));
    }

    [Fact]
    public void SetValue_NullValue_Throws()
    {
        var table = new ParameterValueTable();

        Assert.Throws<ArgumentNullException>(
            () => table.SetValue("serverName", null!));
    }
}
