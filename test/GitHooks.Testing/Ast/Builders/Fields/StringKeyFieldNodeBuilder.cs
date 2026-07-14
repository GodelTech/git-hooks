using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Testing.Ast.Builders.Expressions;

namespace GitHooks.Testing.Ast.Builders.Fields;

public sealed class StringKeyFieldNodeBuilder
    : ValueFieldNodeBuilder<StringKeyFieldNodeBuilder, StringKeyFieldNode<ExpressionNode>, ExpressionNode>
{
    private string? _key;

    public static StringKeyFieldNode<ExpressionNode> Create(
        string key,
        string value = "test")
    {
        return new StringKeyFieldNodeBuilder()
            .WithKey(key)
            .WithValue(value)
            .Build();
    }

    public StringKeyFieldNodeBuilder WithKey(
        string key)
    {
        _key = key;

        return Self;
    }

    public StringKeyFieldNodeBuilder WithBooleanValue(
        Action<BooleanLiteralExpressionNodeBuilder> configure)
    {
        return WithValue(
            Configure(configure).Build());
    }

    public StringKeyFieldNodeBuilder WithIntegerValue(
        Action<IntegerLiteralExpressionNodeBuilder> configure)
    {
        return WithValue(
            Configure(configure).Build());
    }

    public StringKeyFieldNodeBuilder WithStringValue(
        Action<StringLiteralExpressionNodeBuilder> configure)
    {
        return WithValue(
            Configure(configure).Build());
    }

    public StringKeyFieldNodeBuilder WithInterpolatedStringValue(
        Action<InterpolatedStringExpressionNodeBuilder> configure)
    {
        return WithValue(
            Configure(configure).Build());
    }

    public StringKeyFieldNodeBuilder WithValue(
        string value)
    {
        return WithStringValue(x => x.WithValue(value));
    }

    public override StringKeyFieldNode<ExpressionNode> Build()
    {
        if (_key is null)
        {
            throw new InvalidOperationException(
                "Key is required to build a StringKeyFieldNode.");
        }

        if (Value is null)
        {
            throw new InvalidOperationException(
                "Value is required to build a StringKeyFieldNode.");
        }

        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = _key,
            Value = Value,
            Span = Span
        };
    }
}
