using GitHooks.Domain.Ast.Values;

namespace GitHooks.Testing.Ast.Builders.Values;

public sealed class SequenceNodeBuilder
    : ValueNodeBuilder<SequenceNodeBuilder, SequenceNode>
{
    private readonly List<ValueNode> _items = [];

    public SequenceNodeBuilder WithScalarItem(
        Action<ScalarNodeBuilder> configure)
    {
        return WithItem(
            Configure(configure).Build());
    }

    public SequenceNodeBuilder WithMappingItem(
        Action<MappingNodeBuilder> configure)
    {
        return WithItem(
            Configure(configure).Build());
    }

    public SequenceNodeBuilder WithItem(
        string value)
    {
        return WithScalarItem(x => x.WithValue(value));
    }

    public override SequenceNode Build()
    {
        return new SequenceNode()
        {
            Items = [.. _items],
            Span = Span
        };
    }

    private SequenceNodeBuilder WithItem(
        ValueNode item)
    {
        _items.Add(item);

        return Self;
    }
}
