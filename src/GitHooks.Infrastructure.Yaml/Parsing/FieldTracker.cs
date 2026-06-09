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

        // TODO: add public IReadOnlyList<DiagnosticRelatedLocation> RelatedLocations into Diagnostic
        // and use it to report the location of the previous field with the same name.
        // error GH1001: Duplicate 'steps' field.
        // --> pipeline.yml:10:1
        // duplicate declaration
        // note:
        // first declaration
        // --> pipeline.yml:2:1
        if (_fields.TryGetValue(fieldName, out var _))
        {
            throw cursor.CreateException(
                $"Duplicate '{fieldName}' field.");
        }

        _fields.Add(
            fieldName,
            cursor.CurrentSpan());
    }
}
