using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class MappingFields(
    UnknownNodeParser unknownNodeParser)
{
    private readonly UnknownNodeParser _unknownNodeParser =
        unknownNodeParser
        ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    private readonly Dictionary<string, SourceSpan> _fields =
        new(StringComparer.Ordinal);

    private readonly List<UnknownFieldNode> _unknownFields = [];

    public IReadOnlyList<UnknownFieldNode> UnknownFields =>
        _unknownFields;

    public void MarkSeen(
        string fieldName,
        YamlParserCursor cursor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentNullException.ThrowIfNull(cursor);

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

    public void AddUnknownField(
        Scalar key,
        YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(cursor);

        _unknownFields.Add(
            _unknownNodeParser.ParseField(
                key,
                cursor));
    }
}
