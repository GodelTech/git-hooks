namespace GitHooks.Domain.Ast.Fields;

public abstract class ValueFieldNode<TValue>
    : FieldNode
    where TValue : AstNode
{
    public required TValue Value { get; init; }
}
