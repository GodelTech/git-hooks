using GitHooks.Domain.Common;

namespace GitHooks.Domain.Tests.Common;

public sealed class SourcePositionTests
{
    [Fact]
    public void Unknown_HasExpectedSentinelValues()
    {
        Assert.Equal(-1, SourcePosition.Unknown.Line);
        Assert.Equal(-1, SourcePosition.Unknown.Column);
    }

    [Fact]
    public void HasKnownLine_WhenLineIsNegative_ReturnsFalse()
    {
        var position = new SourcePosition(-1, 10);

        Assert.False(position.HasKnownLine);
    }

    [Fact]
    public void HasKnownLine_WhenLineIsZeroOrGreater_ReturnsTrue()
    {
        var position = new SourcePosition(0, -1);

        Assert.True(position.HasKnownLine);
    }

    [Fact]
    public void HasKnownColumn_WhenColumnIsNegative_ReturnsFalse()
    {
        var position = new SourcePosition(10, -1);

        Assert.False(position.HasKnownColumn);
    }

    [Fact]
    public void HasKnownColumn_WhenColumnIsZeroOrGreater_ReturnsTrue()
    {
        var position = new SourcePosition(-1, 0);

        Assert.True(position.HasKnownColumn);
    }

    [Fact]
    public void IsUnknown_WhenLineAndColumnAreUnknown_ReturnsTrue()
    {
        var position = SourcePosition.Unknown;

        Assert.True(position.IsUnknown);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(100, 200)]
    public void IsUnknown_WhenAtLeastOneDimensionIsKnown_ReturnsFalse(long line, long column)
    {
        var position = new SourcePosition(line, column);

        Assert.False(position.IsUnknown);
    }
}
