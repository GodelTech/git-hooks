using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Values;

public sealed class SequenceNode
    : ValueNode
{
    public override AstNodeKind Kind
        => AstNodeKind.Sequence;

    public required IReadOnlyList<ValueNode> Items { get; init; }

    public override void Accept(
        IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(
        IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
