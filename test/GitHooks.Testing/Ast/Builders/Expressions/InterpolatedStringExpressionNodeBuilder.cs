using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class InterpolatedStringExpressionNodeBuilder
    : ExpressionNodeBuilder<InterpolatedStringExpressionNodeBuilder, InterpolatedStringExpressionNode>
{
    private readonly List<ExpressionNode> _parts = [];

    public InterpolatedStringExpressionNodeBuilder WithStringPart(
        Action<StringLiteralExpressionNodeBuilder> configure)
    {
        return WithPart(
            Configure(configure).Build());
    }

    public InterpolatedStringExpressionNodeBuilder WithVariablePart(
        Action<VariableExpressionNodeBuilder> configure)
    {
        return WithPart(
            Configure(configure).Build());
    }

    public InterpolatedStringExpressionNodeBuilder WithVariablePart(
        string path)
    {
        return WithVariablePart(x => x.WithPath(path));
    }

    public InterpolatedStringExpressionNodeBuilder WithPart(
        string value)
    {
        return WithStringPart(x => x.WithValue(value));
    }

    public override InterpolatedStringExpressionNode Build()
    {
        return new InterpolatedStringExpressionNode()
        {
            Parts = [.. _parts],
            Span = Span
        };
    }

    private InterpolatedStringExpressionNodeBuilder WithPart(
        ExpressionNode part)
    {
        _parts.Add(part);

        return Self;
    }
}
