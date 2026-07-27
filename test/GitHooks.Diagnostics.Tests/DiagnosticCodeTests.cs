namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticCodeTests
{
    [Fact]
    public void Create_ReturnsFormattedCode()
    {
        var result = DiagnosticCode.Create(42);

        Assert.Equal(
            "GH0042",
            result.Value);
    }

    [Fact]
    public void Create_Zero_ReturnsFormattedCode()
    {
        var result = DiagnosticCode.Create(0);

        Assert.Equal(
            "GH0000",
            result.Value);
    }

    [Fact]
    public void Create_NegativeValue_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => DiagnosticCode.Create(-1));

        Assert.Equal(
            "value",
            exception.ParamName);
    }
}
