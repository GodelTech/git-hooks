using GitHooks.Diagnostics.Rendering;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests.Rendering;

public sealed class SourcePositionRendererTests
{
    [Fact]
    public void Render_UnknownPositionProvided_ReturnsUnknown()
    {
        var result = SourcePositionRenderer.Render(SourcePosition.Unknown);

        Assert.Equal(
            "<unknown>",
            result);
    }

    [Fact]
    public void Render_FullyKnownPositionProvided_ReturnsFormattedPosition()
    {
        var position = new SourcePosition(10, 20);

        var result = SourcePositionRenderer.Render(position);

        Assert.Equal(
            "10:20",
            result);
    }

    [Fact]
    public void Render_UnknownLineProvided_ReturnsUnknownLine()
    {
        var position = new SourcePosition(-1, 20);

        var result = SourcePositionRenderer.Render(position);

        Assert.Equal(
            "?:20",
            result);
    }

    [Fact]
    public void Render_UnknownColumnProvided_ReturnsUnknownColumn()
    {
        var position = new SourcePosition(10, -1);

        var result = SourcePositionRenderer.Render(position);

        Assert.Equal(
            "10:?",
            result);
    }

    [Fact]
    public void Render_UnknownLineAndColumnProvided_ReturnsUnknown()
    {
        var position = new SourcePosition(-1, -1);

        var result = SourcePositionRenderer.Render(position);

        Assert.Equal(
            "<unknown>",
            result);
    }
}
