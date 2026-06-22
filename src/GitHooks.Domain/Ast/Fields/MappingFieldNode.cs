using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Ast.Fields;

public sealed class MappingFieldNode<TField>
    : FieldNode
    where TField : FieldNode
{
    public override AstNodeKind Kind
        => AstNodeKind.MappingField;

    public required string Key { get; init; }

    public required IReadOnlyList<TField> Fields { get; init; }

    public override void Accept(
        IAstCommandVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override TResult Accept<TResult>(
        IAstQueryVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }
}
