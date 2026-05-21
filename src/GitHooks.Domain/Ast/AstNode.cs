using GitHooks.Domain.Ast.Visitors;
using GitHooks.Domain.Common;

namespace GitHooks.Domain.Ast;

public abstract record AstNode
{
    public abstract AstNodeKind Kind { get; }

    public required SourceSpan Span { get; init; }

    public abstract void Accept(
        IAstCommandVisitor visitor);

    public abstract TResult Accept<TResult>(
        IAstQueryVisitor<TResult> visitor);
}
