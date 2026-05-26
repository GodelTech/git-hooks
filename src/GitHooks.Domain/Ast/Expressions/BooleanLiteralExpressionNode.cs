using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Expressions;

public sealed record BooleanLiteralExpressionNode
    : ExpressionNode
{
    public override AstNodeKind Kind
        => AstNodeKind.BooleanLiteralExpression;

    public required bool Value { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
