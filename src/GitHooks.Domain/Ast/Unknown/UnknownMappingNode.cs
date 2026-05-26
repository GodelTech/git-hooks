using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Unknown;

public sealed record UnknownMappingNode
    : UnknownNode
{
    public override AstNodeKind Kind
        => AstNodeKind.UnknownMapping;

    public required IReadOnlyList<UnknownFieldNode> Fields { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.VisitUnknownNode(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.VisitUnknownNode(this);
    }
}
