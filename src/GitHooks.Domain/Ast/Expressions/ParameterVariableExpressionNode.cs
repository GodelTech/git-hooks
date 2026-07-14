using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Expressions;

public sealed class ParameterVariableExpressionNode
    : VariableExpressionNode
{
    public override AstNodeKind Kind
        => AstNodeKind.ParameterVariableExpression;

    public required string Name { get; init; }

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
