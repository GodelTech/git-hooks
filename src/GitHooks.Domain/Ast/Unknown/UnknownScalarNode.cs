using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Unknown;

public sealed record UnknownScalarNode
    : UnknownNode
{
    public override AstNodeKind Kind =>
        AstNodeKind.UnknownScalar;

    public required string Value { get; init; }

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
