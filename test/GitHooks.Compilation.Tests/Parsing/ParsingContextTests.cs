using GitHooks.Compilation.Parsing;
using GitHooks.Testing.Common;

namespace GitHooks.Compilation.Tests.Parsing;

public sealed class ParsingContextTests
{
    [Fact]
    public void Constructor_NullText_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ParsingContext(
                null!,
                TestSourceDocument.Default));

        Assert.Equal(
            "text",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullDocument_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ParsingContext(
                "test",
                null!));

        Assert.Equal(
            "document",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var document = TestSourceDocument.Default;

        var context = new ParsingContext(
            "test",
            document);

        Assert.Equal(
            "test",
            context.Text);

        Assert.Same(
            document,
            context.Document);
    }
}
