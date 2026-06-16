using System.Collections.ObjectModel;

using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Testing.Ast.Builders;

public abstract class MappingNodeBuilder<TBuilder>
    : AstNodeBuilder<TBuilder>
    where TBuilder : MappingNodeBuilder<TBuilder>
{
    protected Collection<FieldNode> UnknownFields { get; private set; } = [];

    public TBuilder WithUnknownField(FieldNode unknownField)
    {
        ArgumentNullException.ThrowIfNull(unknownField);

        UnknownFields.Add(unknownField);

        return (TBuilder)this;
    }

    public TBuilder WithUnknownField(string key, string value)
    {
        return WithUnknownField(
            TestFields.StringKey(key, value));
    }
}
