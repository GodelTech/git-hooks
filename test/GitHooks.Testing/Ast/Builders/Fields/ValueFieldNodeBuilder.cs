using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Fields;

namespace GitHooks.Testing.Ast.Builders.Fields;

public abstract class ValueFieldNodeBuilder<TBuilder, TNode, TValue>
    : FieldNodeBuilder<TBuilder, TNode>
    where TBuilder : ValueFieldNodeBuilder<TBuilder, TNode, TValue>
    where TNode : ValueFieldNode<TValue>
    where TValue : AstNode
{
    protected TValue? Value { get; set; }

    protected TBuilder WithValue(
        TValue value)
    {
        Value = value;

        return Self;
    }
}
