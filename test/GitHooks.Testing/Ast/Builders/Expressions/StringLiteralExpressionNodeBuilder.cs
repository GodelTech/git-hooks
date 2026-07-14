using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class StringLiteralExpressionNodeBuilder
    : ExpressionNodeBuilder<StringLiteralExpressionNodeBuilder, StringLiteralExpressionNode>
{
    private string? _value;

    public StringLiteralExpressionNodeBuilder WithValue(
        string value)
    {
        _value = value;

        return Self;
    }

    public override StringLiteralExpressionNode Build()
    {
        if (_value is null)
        {
            throw CreateRequiredPropertyException("Value");
        }

        return new StringLiteralExpressionNode()
        {
            Value = _value,
            Span = Span
        };
    }
}
