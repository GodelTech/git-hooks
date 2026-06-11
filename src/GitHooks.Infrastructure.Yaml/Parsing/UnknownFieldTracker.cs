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

    public IReadOnlyList<UnknownFieldNode> GetUnknownFields()
    {
        return _unknownFields;
    }

    public void AddUnknownField(
        ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _unknownFields.Add(
            _unknownNodeParser.ParseField(
                context));
    }

    public void AddUnknownField(
        Scalar key,
        ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        _unknownFields.Add(
            _unknownNodeParser.ParseField(
                key,
                context));
    }
}
