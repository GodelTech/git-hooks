using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Testing.Ast.Builders.Fields;

public sealed class MappingFieldNodeBuilder
    : FieldNodeBuilder<MappingFieldNodeBuilder, MappingFieldNode<StringKeyFieldNode<ExpressionNode>>>
{
    private readonly List<StringKeyFieldNode<ExpressionNode>> _fields = [];

    private string? _key;

    public MappingFieldNodeBuilder WithKey(
        string key)
    {
        _key = key;

        return Self;
    }

    public MappingFieldNodeBuilder WithStringKeyField(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        return WithField(
            Configure(configure).Build());
    }

    public override MappingFieldNode<StringKeyFieldNode<ExpressionNode>> Build()
    {
        if (_key is null)
        {
            throw new InvalidOperationException(
                "Key is required to build a MappingFieldNode.");
        }

        return new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
        {
            Key = _key,
            Fields = [.. _fields],
            Span = Span
        };
    }

    private MappingFieldNodeBuilder WithField(
        StringKeyFieldNode<ExpressionNode> field)
    {
        _fields.Add(field);

        return Self;
    }
}
