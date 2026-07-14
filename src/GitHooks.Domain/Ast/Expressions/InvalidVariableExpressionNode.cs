using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Expressions;

public sealed class InvalidVariableExpressionNode
    : VariableExpressionNode
{
    public override AstNodeKind Kind
        => AstNodeKind.InvalidVariableExpression;

    public required string Text { get; init; }

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
