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

    public InterpolatedStringExpressionNodeBuilder WithParameterVariablePart(
        Action<ParameterVariableExpressionNodeBuilder> configure)
    {
        return WithPart(
            Configure(configure).Build());
    }

    public InterpolatedStringExpressionNodeBuilder WithParameterVariablePart(
        string name)
    {
        return WithParameterVariablePart(x => x.WithName(name));
    }

    public InterpolatedStringExpressionNodeBuilder WithInvalidVariablePart(
        Action<InvalidVariableExpressionNodeBuilder> configure)
    {
        return WithPart(
            Configure(configure).Build());
    }

    public InterpolatedStringExpressionNodeBuilder WithInvalidVariablePart(
        string text)
    {
        return WithInvalidVariablePart(x => x.WithText(text));
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
