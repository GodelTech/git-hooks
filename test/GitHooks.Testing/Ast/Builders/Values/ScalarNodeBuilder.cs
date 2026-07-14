using GitHooks.Domain.Ast.Values;

namespace GitHooks.Testing.Ast.Builders.Values;

public sealed class ScalarNodeBuilder
    : ValueNodeBuilder<ScalarNodeBuilder, ScalarNode>
{
    private string? _value;

    public ScalarNodeBuilder WithValue(
        string value)
    {
        _value = value;

        return Self;
    }

    public override ScalarNode Build()
    {
        if (_value is null)
        {
            throw CreateRequiredPropertyException("Value");
        }

        return new ScalarNode()
        {
            Value = _value,
            Span = Span
        };
    }
}
