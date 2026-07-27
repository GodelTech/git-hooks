using GitHooks.Domain.Common;

namespace GitHooks.Domain.Tests.Common;

public sealed class SourceSpanTests
{
    [Fact]
    public void HasDocument_WhenDocumentProvided_ReturnsTrue()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yml"),
            new SourcePosition(1, 1),
            new SourcePosition(1, 1));

        Assert.True(span.HasDocument);
    }

    [Fact]
    public void HasDocument_WhenDocumentMissing_ReturnsFalse()
    {
        var span = new SourceSpan(
            null,
            new SourcePosition(1, 1),
            new SourcePosition(1, 1));

        Assert.False(span.HasDocument);
    }

    [Fact]
    public void Unknown_ReturnsUnknownSpan()
    {
        var unknownSpan = SourceSpan.Unknown;

        Assert.Null(unknownSpan.Document);
        Assert.Equal(SourcePosition.Unknown, unknownSpan.Start);
        Assert.Equal(SourcePosition.Unknown, unknownSpan.End);
        Assert.False(unknownSpan.HasDocument);
        Assert.True(unknownSpan.HasUnknownPosition);
    }

    [Fact]
    public void HasUnknownPosition_WhenStartAndEndAreUnknown_ReturnsTrue()
    {
        var span = SourceSpan.Unknown;

        Assert.True(span.HasUnknownPosition);
    }

    [Theory]
    [InlineData(0, 0, -1, -1)]
    [InlineData(-1, -1, 0, 0)]
    [InlineData(0, 1, 2, 3)]
    public void HasUnknownPosition_WhenAtLeastOnePositionIsKnown_ReturnsFalse(
        long startLine,
        long startColumn,
        long endLine,
        long endColumn)
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(startLine, startColumn),
            new SourcePosition(endLine, endColumn));

        Assert.False(span.HasUnknownPosition);
    }

    [Fact]
    public void Combine_DifferentDocuments_Throws()
    {
        var start = new SourceSpan(
            new SourceDocument("pipeline.yml"),
            new SourcePosition(1, 1),
            new SourcePosition(1, 5));

        var end = new SourceSpan(
            new SourceDocument("build.yml"),
            new SourcePosition(3, 1),
            new SourcePosition(3, 10));

        var exception = Assert.Throws<ArgumentException>(
            () => SourceSpan.Combine(start, end));

        Assert.Equal(
            "Cannot combine spans from different documents.",
            exception.Message);
    }

    [Fact]
    public void Combine_SameDocument_ReturnsCombinedSpan()
    {
        var document = new SourceDocument("pipeline.yaml");

        var start = new SourceSpan(
            document,
            new SourcePosition(1, 2),
            new SourcePosition(3, 4));

        var end = new SourceSpan(
            document,
            new SourcePosition(5, 6),
            new SourcePosition(7, 8));

        var result = SourceSpan.Combine(start, end);

        Assert.Equal(document, result.Document);
        Assert.Equal(start.Start, result.Start);
        Assert.Equal(end.End, result.End);
    }

    [Fact]
    public void Combine_SameDocumentNameDifferentInstances_ReturnsCombinedSpan()
    {
        var start = new SourceSpan(
            new SourceDocument("pipeline.yml"),
            new SourcePosition(1, 1),
            new SourcePosition(1, 5));

        var end = new SourceSpan(
            new SourceDocument("pipeline.yml"),
            new SourcePosition(3, 1),
            new SourcePosition(3, 10));

        var result = SourceSpan.Combine(start, end);

        Assert.Equal(new SourceDocument("pipeline.yml"), result.Document);
        Assert.Equal(start.Start, result.Start);
        Assert.Equal(end.End, result.End);
    }

    [Fact]
    public void Combine_UnknownAndKnownSpan_Throws()
    {
        var span = new SourceSpan(
            new SourceDocument("pipeline.yml"),
            new SourcePosition(1, 1),
            new SourcePosition(1, 5));

        var exception = Assert.Throws<ArgumentException>(
            () => SourceSpan.Combine(
                SourceSpan.Unknown,
                span));

        Assert.Equal(
            "Cannot combine spans from different documents.",
            exception.Message);
    }

    [Fact]
    public void Combine_UnknownSpans_ReturnsUnknownSpan()
    {
        var result = SourceSpan.Combine(
            SourceSpan.Unknown,
            SourceSpan.Unknown);

        Assert.Equal(
            SourceSpan.Unknown,
            result);
    }
}
