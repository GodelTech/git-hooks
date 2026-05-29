using GitHooks.Domain.Ast.Unknown;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class MappingFields(
    UnknownNodeParser unknownNodeParser)
{
    private readonly FieldTracker _fieldTracker
        = new();

    private readonly UnknownFieldTracker _unknownFieldTracker
        = new(unknownNodeParser);

    public void MarkSeen(Scalar key, YamlParserCursor cursor)
    {
        _fieldTracker.MarkSeen(key, cursor);
    }

    public void AddUnknownField(YamlParserCursor cursor)
    {
        _unknownFieldTracker.AddUnknownField(cursor);
    }

    public void AddUnknownField(Scalar key, YamlParserCursor cursor)
    {
        _unknownFieldTracker.AddUnknownField(key, cursor);
    }

    public IReadOnlyList<UnknownFieldNode> GetUnknownFields()
    {
        return _unknownFieldTracker.GetUnknownFields();
    }
}
