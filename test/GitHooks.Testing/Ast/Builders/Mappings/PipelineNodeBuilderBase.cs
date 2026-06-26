using System.Collections.ObjectModel;

using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Testing.Ast.Builders.Fields;

namespace GitHooks.Testing.Ast.Builders.Mappings;

public abstract class PipelineNodeBuilderBase<TBuilder, TNode>
    : AstNodeBuilder<TBuilder, TNode>
    where TBuilder : PipelineNodeBuilderBase<TBuilder, TNode>
    where TNode : PipelineNodeBase
{
    protected Collection<FieldNode> UnknownFields { get; private set; }
        = [];

    public TBuilder WithUnknownStringKeyField(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        return WithUnknownField(
            Configure(configure).Build());
    }

    public TBuilder WithUnknownComplexKeyField(
        Action<ComplexKeyFieldNodeBuilder> configure)
    {
        return WithUnknownField(
            Configure(configure).Build());
    }

    public TBuilder WithUnknownField(
        string key,
        string value)
    {
        return WithUnknownStringKeyField(x => x
            .WithKey(key)
            .WithStringValue(v => v.WithValue(value)));
    }

    private TBuilder WithUnknownField(
        FieldNode unknownField)
    {
        UnknownFields.Add(unknownField);

        return Self;
    }
}
