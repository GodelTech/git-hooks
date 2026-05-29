using GitHooks.Domain.Ast.Unknown;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class UnknownFieldTracker(
    UnknownNodeParser unknownNodeParser)
{
    private readonly UnknownNodeParser _unknownNodeParser
        = unknownNodeParser ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    private readonly List<UnknownFieldNode> _unknownFields
        = [];

    public void AddUnknownField(
        YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        _unknownFields.Add(
            _unknownNodeParser.ParseField(
                cursor));
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

    public IReadOnlyList<UnknownFieldNode> GetUnknownFields()
    {
        return _unknownFields;
    }
}
