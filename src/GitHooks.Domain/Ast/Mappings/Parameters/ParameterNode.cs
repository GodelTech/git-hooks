using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Parameters;

public sealed record ParameterNode
    : PipelineNodeBase
{
    public override AstNodeKind Kind
        => AstNodeKind.Parameter;

    public required string Name { get; init; }

    public string? DisplayName { get; init; }

    public ParameterType Type { get; init; }

    public ExpressionNode? DefaultValue { get; init; }

    public IReadOnlyList<ExpressionNode> Values { get; init; }
        = [];

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
