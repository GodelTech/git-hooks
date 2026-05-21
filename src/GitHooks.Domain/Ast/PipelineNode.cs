using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast;

public sealed record PipelineNode
    : AstNode
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
