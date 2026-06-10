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
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DiagnosticCode.Create(-1));
    }
}
