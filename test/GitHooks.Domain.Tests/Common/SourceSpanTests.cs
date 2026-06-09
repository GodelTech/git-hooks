using GitHooks.Domain.Common;

namespace GitHooks.Domain.Tests.Common;

public sealed class SourceSpanTests
{
    [Fact]
    public void Unknown_HasUnknownStartAndEnd()
    {
        Assert.Equal(SourcePosition.Unknown, SourceSpan.Unknown.Start);
        Assert.Equal(SourcePosition.Unknown, SourceSpan.Unknown.End);
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
    public void HasUnknownPosition_WhenAtLeastOneBoundaryIsKnown_ReturnsFalse(
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
    public void Combine_ReturnsSpanFromFirstStartToSecondEnd()
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
}
