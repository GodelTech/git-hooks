using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class IntegerLiteralExpressionNodeBuilder
    : ExpressionNodeBuilder<IntegerLiteralExpressionNodeBuilder, IntegerLiteralExpressionNode>
{
    private int? _value;

    public IntegerLiteralExpressionNodeBuilder WithValue(
        int value)
    {
        _value = value;

        return Self;
    }

    public override IntegerLiteralExpressionNode Build()
    {
        if (_value is null ||
            !_value.HasValue)
        {
            throw new InvalidOperationException(
                "Value is required to build an IntegerLiteralExpressionNode.");
        }

        return new IntegerLiteralExpressionNode()
        {
            Value = _value.Value,
            Span = Span
        };
    }
}
