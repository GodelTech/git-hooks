using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class InvalidVariableExpressionNodeBuilder
    : ExpressionNodeBuilder<InvalidVariableExpressionNodeBuilder, InvalidVariableExpressionNode>
{
    private string? _text;

    public InvalidVariableExpressionNodeBuilder WithText(
        string text)
    {
        _text = text;

        return Self;
    }

    public override InvalidVariableExpressionNode Build()
    {
        if (_text is null)
        {
            throw CreateRequiredPropertyException("Text");
        }

        return new InvalidVariableExpressionNode()
        {
            Text = _text,
            Span = Span
        };
    }
}
