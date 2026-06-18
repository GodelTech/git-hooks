using GitHooks.Infrastructure.Yaml.Parsing;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Testing;

internal static class YamlParserCursorExtensions
{
    public static void StartDocument(
        this YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        _ = cursor.Read<StreamStart>();
        _ = cursor.Read<DocumentStart>();
    }

    public static void EndDocument(
        this YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        _ = cursor.Read<DocumentEnd>();
        _ = cursor.Read<StreamEnd>();
    }
}
