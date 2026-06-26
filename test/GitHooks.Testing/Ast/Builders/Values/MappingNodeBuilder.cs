using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Testing.Ast.Builders.Fields;

namespace GitHooks.Testing.Ast.Builders.Values;

public sealed class MappingNodeBuilder
    : ValueNodeBuilder<MappingNodeBuilder, MappingNode>
{
    private readonly List<FieldNode> _fields = [];

    public MappingNodeBuilder WithStringKeyField(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        return WithField(
            Configure(configure).Build());
    }

    public MappingNodeBuilder WithField(
        string key,
        string value)
    {
        return WithStringKeyField(x => x
            .WithKey(key)
            .WithValue(value));
    }

    public override MappingNode Build()
    {
        return new MappingNode()
        {
            Fields = [.. _fields],
            Span = Span
        };
    }

    private MappingNodeBuilder WithField(
        FieldNode field)
    {
        _fields.Add(field);

        return Self;
    }
}
