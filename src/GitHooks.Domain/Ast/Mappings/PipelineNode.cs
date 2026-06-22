using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings;

public sealed class PipelineNode
    : PipelineNodeBase
{
    public override AstNodeKind Kind
        => AstNodeKind.Pipeline;

    public required IReadOnlyList<ParameterNode> Parameters { get; init; }

    public required IReadOnlyList<StepNode> Steps { get; init; }

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
