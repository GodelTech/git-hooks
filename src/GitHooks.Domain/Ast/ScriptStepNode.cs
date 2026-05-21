using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast;

public sealed record ScriptStepNode
    : StepNode
{
    public override AstNodeKind Kind
        => AstNodeKind.ScriptStep;

    public required ExpressionNode Script { get; init; }

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
