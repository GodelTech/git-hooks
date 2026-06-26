using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class BooleanLiteralExpressionNodeBuilder
    : ExpressionNodeBuilder<BooleanLiteralExpressionNodeBuilder, BooleanLiteralExpressionNode>
{
    private bool? _value;

    public BooleanLiteralExpressionNodeBuilder WithValue(
        bool value)
    {
        _value = value;

        return Self;
    }

    public override BooleanLiteralExpressionNode Build()
    {
        if (_value is null ||
            !_value.HasValue)
        {
            throw new InvalidOperationException(
                "Value is required to build a BooleanLiteralExpressionNode.");
        }

        return new BooleanLiteralExpressionNode()
        {
            Value = _value.Value,
            Span = Span
        };
    }
}
