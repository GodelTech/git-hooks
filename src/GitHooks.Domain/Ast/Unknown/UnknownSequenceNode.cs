using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Unknown;

public sealed record UnknownSequenceNode
    : UnknownNode
{
    public override AstNodeKind Kind =>
        AstNodeKind.UnknownSequence;

    public required IReadOnlyList<UnknownNode> Items { get; init; }

    public override void Accept(
        IAstCommandVisitor visitor)
    {
        visitor.VisitUnknownNode(this);
    }

    public override TResult Accept<TResult>(
        IAstQueryVisitor<TResult> visitor)
    {
        return visitor.VisitUnknownNode(this);
    }
}
