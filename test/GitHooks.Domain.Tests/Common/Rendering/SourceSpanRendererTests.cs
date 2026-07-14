using GitHooks.Domain.Common;
using GitHooks.Domain.Common.Rendering;

namespace GitHooks.Domain.Tests.Common.Rendering;

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
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(1, 5),
            new SourcePosition(1, 10));

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(1:5-1:10)",
            result);
    }

    [Fact]
    public void Render_UnknownStartPositionProvided_ReturnsFormattedSpan()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            SourcePosition.Unknown,
            new SourcePosition(2, 5));

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(<unknown>-2:5)",
            result);
    }

    [Fact]
    public void Render_UnknownEndPositionProvided_ReturnsFormattedSpan()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(1, 5),
            SourcePosition.Unknown);

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(1:5-<unknown>)",
            result);
    }

    [Fact]
    public void Render_KnownSpanWithoutDocument_ReturnsFormattedSpan()
    {
        var span = new SourceSpan(
            null,
            new SourcePosition(1, 5),
            new SourcePosition(1, 10));

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "(1:5-1:10)",
            result);
    }

    [Fact]
    public void Render_UnknownStartAndEndWithoutDocument_ReturnsUnknownSpan()
    {
        var span = new SourceSpan(
            null,
            SourcePosition.Unknown,
            SourcePosition.Unknown);

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "<unknown>",
            result);
    }

    [Fact]
    public void Render_UnknownSpanWithDocument_ReturnsDocumentAndUnknown()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            SourcePosition.Unknown,
            SourcePosition.Unknown);

        var result = SourceSpanRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(<unknown>)",
            result);
    }
}
