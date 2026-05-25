using GitHooks.Domain.Ast.Unknown;

namespace GitHooks.Domain.Ast.Mappings;

public abstract record MappingNode
    : AstNode
{
    public required IReadOnlyList<UnknownFieldNode> UnknownFields { get; init; }
}
