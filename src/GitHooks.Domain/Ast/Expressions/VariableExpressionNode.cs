using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Expressions;

public sealed record VariableExpressionNode
    : ExpressionNode
{
    public override AstNodeKind Kind
        => AstNodeKind.VariableExpression;

    public required string Path { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
