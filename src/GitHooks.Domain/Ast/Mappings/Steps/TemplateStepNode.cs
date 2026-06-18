using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Steps;

public sealed record TemplateStepNode
    : StepNode
{
    public override AstNodeKind Kind
        => AstNodeKind.TemplateStep;

    public required StringKeyFieldNode<ExpressionNode> Template { get; init; }

    public MappingFieldNode<StringKeyFieldNode<ExpressionNode>>? Parameters { get; init; }

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
