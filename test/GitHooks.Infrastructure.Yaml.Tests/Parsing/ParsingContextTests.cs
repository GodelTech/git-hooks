using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class ParsingContextTests
{
    [Fact]
    public void Constructor_NullYaml_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new ParsingContext(
                    null!,
                    new SourceDocument("test.yaml")));

        Assert.Equal(
            "yaml",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyYaml_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () => new ParsingContext(
                    string.Empty,
                    new SourceDocument("test.yaml")));

        Assert.Equal(
            "yaml",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullSourceDocument_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new ParsingContext(
                    "{}",
                    null!));

        Assert.Equal(
            "sourceDocument",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_CreatesCursor()
    {
        var context =
            new ParsingContext(
                "{}",
                new SourceDocument("test.yaml"));

        Assert.NotNull(
            context.Cursor);
    }

    [Fact]
    public void Diagnostics_InitiallyEmpty()
    {
        var context =
            new ParsingContext(
                "{}",
                new SourceDocument("test.yaml"));

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Report_AddsDiagnostic()
    {
        var context =
            new ParsingContext(
                "{}",
                new SourceDocument("test.yaml"));

        context.Report(
            DiagnosticDescriptors.InvalidYaml,
            SourceSpan.Unknown,
            "Test");

        var diagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml,
            diagnostic.Descriptor);

        Assert.Equal(
            SourceSpan.Unknown,
            diagnostic.Span);

        Assert.Equal(
            "Test",
            diagnostic.Arguments[0]);
    }
}
