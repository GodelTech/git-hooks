using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Steps;

public sealed record InvalidStepNode
    : StepNode
{
    public override AstNodeKind Kind
        => AstNodeKind.InvalidStep;

    public required IReadOnlyList<FieldNode> Fields { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
