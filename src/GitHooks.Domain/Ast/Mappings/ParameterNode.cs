using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings;

public sealed record ParameterNode
    : MappingNode
{
    public override AstNodeKind Kind
        => AstNodeKind.Parameter;

    public required string Name { get; init; }

    public required ExpressionNode Value { get; init; }

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
