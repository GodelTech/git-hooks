using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;

namespace GitHooks.Infrastructure.Yaml.Tests;

internal static class TestParsingContextFactory
{
    public static ParsingContext CreateEmpty(
        SourceDocument document)
    {
        return Create(
            "{}",
            document);
    }

    public static ParsingContext Create(
        string yaml,
        SourceDocument document)
    {
        return new ParsingContext(
            yaml,
            document);
    }
}
