using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Unknown;

public sealed record UnknownFieldNode
    : UnknownNode
{
    public override AstNodeKind Kind =>
        AstNodeKind.UnknownField;

    public required string Key { get; init; }

    public required UnknownNode Value { get; init; }

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
