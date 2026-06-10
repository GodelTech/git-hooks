using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticLocationTests
{
    [Fact]
    public void Create_NullMessage_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => DiagnosticLocation.Create(
                SourceSpan.Unknown,
                null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_EmptyMessage_Throws(
        string message)
    {
        Assert.Throws<ArgumentException>(
            () => DiagnosticLocation.Create(
                SourceSpan.Unknown,
                message));
    }

    [Fact]
    public void Create_ReturnsLocation()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(1, 1),
            new SourcePosition(1, 10));

        var result = DiagnosticLocation.Create(
            span,
            "First declaration is here.");

        Assert.Equal(
            span,
            result.Span);

        Assert.Equal(
            "First declaration is here.",
            result.Message);
    }
}
