using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Testing.Ast.Builders.Expressions;

public abstract class ExpressionNodeBuilder<TBuilder, TNode>
    : AstNodeBuilder<TBuilder, TNode>
    where TBuilder : ExpressionNodeBuilder<TBuilder, TNode>
    where TNode : ExpressionNode;
