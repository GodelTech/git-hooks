using GitHooks.Domain.Ast.Values;

namespace GitHooks.Testing.Ast.Builders.Values;

public abstract class ValueNodeBuilder<TBuilder, TNode>
    : AstNodeBuilder<TBuilder, TNode>
    where TBuilder : ValueNodeBuilder<TBuilder, TNode>
    where TNode : ValueNode;
