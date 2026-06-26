using GitHooks.Domain.Ast;
using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

namespace GitHooks.Testing.Ast.Builders;

public abstract class AstNodeBuilder<TBuilder, TNode>
    where TBuilder : AstNodeBuilder<TBuilder, TNode>
    where TNode : AstNode
{
    protected TBuilder Self
        => (TBuilder)this;

    protected SourceSpan Span { get; private set; }
        = TestSourceSpan.Unknown;

    public TBuilder WithSpan(
        SourceSpan span)
    {
        Span = span;

        return Self;
    }

    public abstract TNode Build();

    protected static TAstNodeBuilder Configure<TAstNodeBuilder>(
        Action<TAstNodeBuilder> configure)
        where TAstNodeBuilder : new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new TAstNodeBuilder();

        configure(builder);

        return builder;
    }

    // todo: start using method below instead of throwing exceptions in builders
    protected static T Require<T>(
        T? value,
        string name)
        where T : class
    {
        return value
            ?? throw new InvalidOperationException(
                $"{name} is required.");
    }
}
