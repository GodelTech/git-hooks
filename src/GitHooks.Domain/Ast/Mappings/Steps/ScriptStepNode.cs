using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Steps;

public sealed record ScriptStepNode
    : StepNode
{
    public override AstNodeKind Kind
        => AstNodeKind.ScriptStep;

    public required ExpressionNode Script { get; init; }

    public ExpressionNode? DisplayName { get; init; }

    public ExpressionNode? Condition { get; init; }

    public ExpressionNode? TimeoutInMinutes { get; init; }

    public ExpressionNode? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, ExpressionNode> Env { get; init; }
        = new Dictionary<string, ExpressionNode>();

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
