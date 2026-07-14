using GitHooks.Domain.Common.Rendering;

namespace GitHooks.Domain.Tests.Common.Rendering;

public sealed class StringRendererTests
{
    [Fact]
    public void RenderQuoted_WithNullValue_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => StringRenderer.RenderQuoted(null!));

        Assert.Equal(
            "value",
            exception.ParamName);
    }

    [Fact]
    public void RenderQuoted_WithEmptyString_ReturnsQuotedString()
    {
        var result = StringRenderer.RenderQuoted(string.Empty);

        Assert.Equal(
            "\"\"",
            result);
    }

    [Fact]
    public void RenderQuoted_WithPlainText_ReturnsQuotedString()
    {
        var result = StringRenderer.RenderQuoted("hello");

        Assert.Equal(
            "\"hello\"",
            result);
    }

    [Theory]
    [InlineData("\\", "\"\\\\\"")]
    [InlineData("\"", "\"\\\"\"")]
    [InlineData("\n", "\"\\n\"")]
    [InlineData("\r", "\"\\r\"")]
    [InlineData("\t", "\"\\t\"")]
    public void RenderQuoted_EscapesSpecialCharacters(
        string value,
        string expected)
    {
        var result = StringRenderer.RenderQuoted(value);

        Assert.Equal(
            expected,
            result);
    }

    [Fact]
    public void RenderQuoted_EscapesMultipleCharacters()
    {
        var result = StringRenderer.RenderQuoted("a\\b\"c\nd\re\tf");

        Assert.Equal(
            "\"a\\\\b\\\"c\\nd\\re\\tf\"",
            result);
    }
}
