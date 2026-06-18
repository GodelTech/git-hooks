using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Values;

public sealed record ScalarNode
    : ValueNode
{
    public override AstNodeKind Kind
        => AstNodeKind.Scalar;

    public required string Value { get; init; }

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
