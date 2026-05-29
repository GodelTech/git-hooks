using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Steps;

public sealed record TemplateStepNode
    : StepNode
{
    public override AstNodeKind Kind
        => AstNodeKind.TemplateStep;

    public required ExpressionNode Template { get; init; }

    public IReadOnlyDictionary<string, ExpressionNode> Parameters { get; init; }
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
