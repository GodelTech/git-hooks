namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticCodeTests
{
    [Fact]
    public void EqualValues_AreEqual()
    {
        var left = new DiagnosticCode("GH0001");
        var right = new DiagnosticCode("GH0001");

        Assert.Equal(left, right);
        Assert.True(left == right);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var code = new DiagnosticCode("GH9999");

        Assert.Equal(
            "GH9999",
            code.ToString());
    }
}
