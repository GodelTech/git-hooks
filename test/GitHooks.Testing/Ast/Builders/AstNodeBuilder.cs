using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast.Builders;

public abstract class AstNodeBuilder<TBuilder>
    where TBuilder : AstNodeBuilder<TBuilder>
{
    protected SourceSpan Span { get; private set; } = SourceSpan.Unknown;

    public TBuilder WithSpan(SourceSpan span)
    {
        Span = span;

        return (TBuilder)this;
    }
}
