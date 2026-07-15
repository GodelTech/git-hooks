using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Testing.Ast.Builders.Expressions;

namespace GitHooks.Testing.Ast.Builders.Fields;

public sealed class SequenceFieldNodeBuilder
    : FieldNodeBuilder<SequenceFieldNodeBuilder, SequenceFieldNode<ExpressionNode>>
{
    private readonly List<ExpressionNode> _items = [];

    private string? _key;

    public SequenceFieldNodeBuilder WithKey(
        string key)
    {
        _key = key;

        return Self;
    }

    public SequenceFieldNodeBuilder WithStringLiteralExpressionItem(
        Action<StringLiteralExpressionNodeBuilder> configure)
    {
        return WithItem(
            Configure(configure).Build());
    }

    public SequenceFieldNodeBuilder WithItems(
        IReadOnlyList<ExpressionNode> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            WithItem(item);
        }

        return Self;
    }

    public override SequenceFieldNode<ExpressionNode> Build()
    {
        if (_key is null)
        {
            throw CreateRequiredPropertyException("Key");
        }

        return new SequenceFieldNode<ExpressionNode>
        {
            Key = _key,
            Items = [.. _items],
            Span = Span
        };
    }

    private SequenceFieldNodeBuilder WithItem(
        ExpressionNode item)
    {
        _items.Add(item);

        return Self;
    }
}
