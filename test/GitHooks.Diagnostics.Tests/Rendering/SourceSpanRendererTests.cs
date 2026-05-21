using GitHooks.Diagnostics.Rendering;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests.Rendering;

public sealed class SourceSpanRendererTests
{
    [Fact]
    public void Render_UnknownSpanProvided_ReturnsUnknown()
    {
        var result = SourceSpanRenderer.Render(
            SourceSpan.Unknown);

        Assert.Equal(
            "<unknown>",
            result);
    }

    [Fact]
    public void Render_FullyKnownSpanProvided_ReturnsFormattedSpan()
    {
        var span = new SourceSpan(
            new SourcePosition(1, 5),
            new SourcePosition(1, 10));

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "(1:5-1:10)",
            result);
    }

    [Fact]
    public void Render_PartiallyKnownStartPositionProvided_ReturnsPartialPosition()
    {
        var span = new SourceSpan(
            new SourcePosition(1, -1),
            new SourcePosition(2, 5));

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "(1:?-2:5)",
            result);
    }

    [Fact]
    public void Render_UnknownStartPositionProvided_ReturnsUnknownPosition()
    {
        var span = new SourceSpan(
            SourcePosition.Unknown,
            new SourcePosition(2, 5));

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "(<unknown>-2:5)",
            result);
    }

    [Fact]
    public void Render_UnknownEndPositionProvided_ReturnsUnknownPosition()
    {
        var span = new SourceSpan(
            new SourcePosition(1, 5),
            SourcePosition.Unknown);

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "(1:5-<unknown>)",
            result);
    }
}
