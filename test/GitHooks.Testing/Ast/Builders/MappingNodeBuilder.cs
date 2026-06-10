using System.Collections.ObjectModel;

using GitHooks.Domain.Ast.Unknown;

namespace GitHooks.Testing.Ast.Builders;

public abstract class MappingNodeBuilder<TBuilder>
    : AstNodeBuilder<TBuilder>
    where TBuilder : MappingNodeBuilder<TBuilder>
{
    protected Collection<UnknownFieldNode> UnknownFields { get; private set; } = [];

    public TBuilder WithUnknownField(
        UnknownFieldNode unknownField)
    {
        ArgumentNullException.ThrowIfNull(unknownField);

        UnknownFields.Add(unknownField);

        return (TBuilder)this;
    }
}
