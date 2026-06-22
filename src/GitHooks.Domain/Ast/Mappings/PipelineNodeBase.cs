using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Domain.Ast.Mappings;

public abstract class PipelineNodeBase
    : AstNode
{
    public required IReadOnlyList<FieldNode> UnknownFields { get; init; }
}
