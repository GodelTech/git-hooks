using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Testing.Common;

namespace GitHooks.Compilation.Tests.Parsing;

public sealed class ParsingContextTests
{
    [Fact]
    public void Constructor_NullText_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ParsingContext(
                null!,
                TestSourceDocument.Default,
                new DiagnosticBag()));
    }

    [Fact]
    public void Constructor_NullDocument_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ParsingContext(
                "test",
                null!,
                new DiagnosticBag()));
    }

    [Fact]
    public void Constructor_NullDiagnostics_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ParsingContext(
                "test",
                TestSourceDocument.Default,
                null!));
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var document = TestSourceDocument.Default;
        var diagnostics = new DiagnosticBag();

        var context = new ParsingContext(
            "test",
            document,
            diagnostics);

        Assert.Equal(
            "test",
            context.Text);

        Assert.Same(
            document,
            context.Document);

        Assert.Same(
            diagnostics,
            context.Diagnostics);
    }
}
