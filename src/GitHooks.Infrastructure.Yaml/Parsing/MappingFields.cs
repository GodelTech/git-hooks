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

    public void MarkSeen(Scalar key, ParsingContext context)
    {
        _fieldTracker.MarkSeen(key, context);
    }

    public IReadOnlyList<UnknownFieldNode> GetUnknownFields()
    {
        return _unknownFieldTracker.GetUnknownFields();
    }

    public void AddUnknownField(ParsingContext context)
    {
        _unknownFieldTracker.AddUnknownField(context);
    }

    public void AddUnknownField(Scalar key, ParsingContext context)
    {
        _unknownFieldTracker.AddUnknownField(key, context);
    }
}
