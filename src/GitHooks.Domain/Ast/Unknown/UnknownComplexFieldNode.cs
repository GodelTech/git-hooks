using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Unknown;

public sealed record UnknownComplexFieldNode
    : UnknownFieldNode
{
    public override AstNodeKind Kind
        => AstNodeKind.UnknownComplexField;

    public required UnknownNode Key { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.VisitUnknownNode(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.VisitUnknownNode(this);
    }
}
