using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Expressions;

public sealed class IntegerLiteralExpressionNode
    : ExpressionNode
{
    public override AstNodeKind Kind
        => AstNodeKind.IntegerLiteralExpression;

    public required int Value { get; init; }

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
