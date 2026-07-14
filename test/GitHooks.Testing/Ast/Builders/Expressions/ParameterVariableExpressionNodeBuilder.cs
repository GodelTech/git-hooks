using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class ParameterVariableExpressionNodeBuilder
    : ExpressionNodeBuilder<ParameterVariableExpressionNodeBuilder, ParameterVariableExpressionNode>
{
    private string? _name;

    public ParameterVariableExpressionNodeBuilder WithName(
        string name)
    {
        _name = name;

        return Self;
    }

    public override ParameterVariableExpressionNode Build()
    {
        if (_name is null)
        {
            throw CreateRequiredPropertyException("Name");
        }

        return new ParameterVariableExpressionNode()
        {
            Name = _name,
            Span = Span
        };
    }
}
