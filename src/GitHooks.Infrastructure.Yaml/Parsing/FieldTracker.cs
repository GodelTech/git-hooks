using GitHooks.Domain.Common;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class FieldTracker
{
    private readonly Dictionary<string, SourceSpan> _fields
        = new(StringComparer.OrdinalIgnoreCase);

    public void MarkSeen(
        Scalar key,
        YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(cursor);

        var fieldName = key.Value;

        if (_fields.TryGetValue(fieldName, out var existing))
        {
            throw cursor.CreateParsingException(
                $"Duplicate '{fieldName}' field. " +
                $"First declared at " +
                $"{existing.Start.Line}:{existing.Start.Column}");
        }

        _fields.Add(
            fieldName,
            cursor.CurrentSpan());
    }
}
