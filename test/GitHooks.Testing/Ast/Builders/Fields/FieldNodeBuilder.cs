using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Testing.Ast.Builders.Fields;

public abstract class FieldNodeBuilder<TBuilder, TNode>
    : AstNodeBuilder<TBuilder, TNode>
    where TBuilder : FieldNodeBuilder<TBuilder, TNode>
    where TNode : FieldNode;
