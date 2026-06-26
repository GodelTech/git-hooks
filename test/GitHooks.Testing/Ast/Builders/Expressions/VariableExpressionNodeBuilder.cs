using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public sealed class VariableExpressionNodeBuilder
    : ExpressionNodeBuilder<VariableExpressionNodeBuilder, VariableExpressionNode>
{
    private string? _path;

    public VariableExpressionNodeBuilder WithPath(
        string path)
    {
        _path = path;

        return Self;
    }

    public override VariableExpressionNode Build()
    {
        if (_path is null)
        {
            throw new InvalidOperationException(
                "Path is required to build a VariableExpressionNode.");
        }

        return new VariableExpressionNode()
        {
            Path = _path,
            Span = Span
        };
    }
}
