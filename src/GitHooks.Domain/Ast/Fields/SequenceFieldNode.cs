using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Fields;

public sealed record SequenceFieldNode<TValue>
    : FieldNode
    where TValue : AstNode
{
    public override AstNodeKind Kind
        => AstNodeKind.SequenceField;

    public required string Key { get; init; }

    public required IReadOnlyList<TValue> Items { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
