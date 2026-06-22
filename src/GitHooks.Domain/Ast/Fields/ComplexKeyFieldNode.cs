using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Fields;

public sealed class ComplexKeyFieldNode<TValue>
    : ValueFieldNode<TValue>
    where TValue : AstNode
{
    public override AstNodeKind Kind
        => AstNodeKind.ComplexKeyField;

    public required ValueNode Key { get; init; }

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
