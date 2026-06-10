using GitHooks.Diagnostics.Printing;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests.Printing;

public sealed class DiagnosticLocationRendererTests
{
    [Fact]
    public void Render_UnknownSpanProvided_ReturnsUnknown()
    {
        var result = DiagnosticLocationRenderer.Render(
            SourceSpan.Unknown);

        Assert.Equal(
            "<unknown>",
            result);
    }

    [Fact]
    public void Render_UnknownSpanWithDocumentProvided_ReturnsDocumentAndUnknown()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            SourcePosition.Unknown,
            SourcePosition.Unknown);

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(<unknown>)",
            result);
    }

    [Fact]
    public void Render_PointSpanProvided_ReturnsPointLocation()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(5, 1),
            new SourcePosition(5, 1));

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(5,1)",
            result);
    }

    [Fact]
    public void Render_RangeSpanProvided_ReturnsRangeLocation()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(5, 1),
            new SourcePosition(5, 10));

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(5,1,5,10)",
            result);
    }

    [Fact]
    public void Render_PointSpanWithoutDocument_ReturnsPointLocation()
    {
        var span = new SourceSpan(
            null,
            new SourcePosition(5, 1),
            new SourcePosition(5, 1));

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "5,1",
            result);
    }

    [Fact]
    public void Render_RangeSpanWithoutDocument_ReturnsRangeLocation()
    {
        var span = new SourceSpan(
            null,
            new SourcePosition(5, 1),
            new SourcePosition(5, 10));

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "5,1,5,10",
            result);
    }

    [Fact]
    public void Render_PartiallyKnownSpanProvided_ReturnsUnknownLocation()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(5, -1),
            new SourcePosition(5, 10));

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "pipeline.yaml(<unknown>)",
            result);
    }

    [Fact]
    public void Render_PartiallyKnownSpanWithoutDocument_ReturnsUnknownLocation()
    {
        var span = new SourceSpan(
            null,
            new SourcePosition(5, -1),
            new SourcePosition(5, 10));

        var result = DiagnosticLocationRenderer.Render(span);

        Assert.Equal(
            "<unknown>",
            result);
    }
}
