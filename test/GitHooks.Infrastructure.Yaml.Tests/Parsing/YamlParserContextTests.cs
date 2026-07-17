using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlParserContextTests
{
    [Fact]
    public void Constructor_NullParsingContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new YamlParserContext(
                    null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_CreatesCursor()
    {
        var context =
            new YamlParserContext(
                new ParsingContext(
                    "{}",
                    new SourceDocument("test.yaml"),
                    new DiagnosticBag()));

        Assert.NotNull(
            context.Cursor);
    }

    [Fact]
    public void Diagnostics_SetDiagnostics()
    {
        var diagnostics = new DiagnosticBag();

        var context =
            new YamlParserContext(
                new ParsingContext(
                    "{}",
                    new SourceDocument("test.yaml"),
                    diagnostics));

        Assert.Equal(
            diagnostics,
            context.Diagnostics);
    }
}
