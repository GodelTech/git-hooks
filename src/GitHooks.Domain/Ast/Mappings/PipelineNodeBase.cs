using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Domain.Ast.Mappings;

public abstract record PipelineNodeBase
    : AstNode
{
    public required IReadOnlyList<FieldNode> UnknownFields { get; init; }
}
