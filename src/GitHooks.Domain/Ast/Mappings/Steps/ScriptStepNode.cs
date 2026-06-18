using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Steps;

public sealed record ScriptStepNode
    : StepNode
{
    public override AstNodeKind Kind
        => AstNodeKind.ScriptStep;

    public required StringKeyFieldNode<ExpressionNode> Script { get; init; }

    public StringKeyFieldNode<ExpressionNode>? DisplayName { get; init; }

    public StringKeyFieldNode<ExpressionNode>? Condition { get; init; }

    public StringKeyFieldNode<ExpressionNode>? TimeoutInMinutes { get; init; }

    public StringKeyFieldNode<ExpressionNode>? WorkingDirectory { get; init; }

    public MappingFieldNode<StringKeyFieldNode<ExpressionNode>>? Env { get; init; }

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
