using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Expressions;

public sealed record InterpolatedStringExpressionNode
    : ExpressionNode
{
    public override AstNodeKind Kind
        => AstNodeKind.InterpolatedStringExpression;

    public required IReadOnlyList<ExpressionNode> Parts { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
