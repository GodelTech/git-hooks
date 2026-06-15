namespace GitHooks.Domain.Ast.Fields;

public abstract record ValueFieldNode<TValue>
    : FieldNode
    where TValue : AstNode
{
    public required TValue Value { get; init; }
}
