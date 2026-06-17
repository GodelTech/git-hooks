using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Mappings.Parameters;

public sealed record ParameterNode
    : PipelineNodeBase
{
    public override AstNodeKind Kind
        => AstNodeKind.Parameter;

    public required StringKeyFieldNode<ExpressionNode> Name { get; init; }

    public StringKeyFieldNode<ExpressionNode>? DisplayName { get; init; }

    // todo: dicide how to migrate into fields
    public ParameterType Type { get; init; }

    public StringKeyFieldNode<ExpressionNode>? DefaultValue { get; init; }

    public SequenceFieldNode<ExpressionNode>? Values { get; init; }

    public override void Accept(IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
