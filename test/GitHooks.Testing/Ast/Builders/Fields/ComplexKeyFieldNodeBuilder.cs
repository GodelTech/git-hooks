using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Testing.Ast.Builders.Values;

namespace GitHooks.Testing.Ast.Builders.Fields;

public sealed class ComplexKeyFieldNodeBuilder
    : ValueFieldNodeBuilder<ComplexKeyFieldNodeBuilder, ComplexKeyFieldNode<AstNode>, AstNode>
{
    private ValueNode? _key;

    public ComplexKeyFieldNodeBuilder WithScalarKey(
        Action<ScalarNodeBuilder> configure)
    {
        return WithKey(
            Configure(configure).Build());
    }

    public ComplexKeyFieldNodeBuilder WithKey(
        string key)
    {
        return WithScalarKey(x => x.WithValue(key));
    }

    public ComplexKeyFieldNodeBuilder WithScalarValue(
        Action<ScalarNodeBuilder> configure)
    {
        return WithValue(
            Configure(configure).Build());
    }

    public ComplexKeyFieldNodeBuilder WithSequenceValue(
        Action<SequenceNodeBuilder> configure)
    {
        return WithValue(
            Configure(configure).Build());
    }

    public override ComplexKeyFieldNode<AstNode> Build()
    {
        if (_key is null)
        {
            throw CreateRequiredPropertyException("Key");
        }

        if (Value is null)
        {
            throw CreateRequiredPropertyException("Value");
        }

        return new ComplexKeyFieldNode<AstNode>
        {
            Key = _key,
            Value = Value,
            Span = Span
        };
    }

    private ComplexKeyFieldNodeBuilder WithKey(
        ValueNode value)
    {
        _key = value;

        return Self;
    }
}
