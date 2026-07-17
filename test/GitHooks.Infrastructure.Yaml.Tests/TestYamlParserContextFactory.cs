using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;

namespace GitHooks.Infrastructure.Yaml.Tests;

internal static class TestYamlParserContextFactory
{
    public static YamlParserContext CreateEmpty(
        SourceDocument document)
    {
        return Create(
            "{}",
            document);
    }

    public static YamlParserContext Create(
        string yaml,
        SourceDocument document)
    {
        return new YamlParserContext(
            new ParsingContext(
                yaml,
                document,
                new DiagnosticBag()));
    }
}
