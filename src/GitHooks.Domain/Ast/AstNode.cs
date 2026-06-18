using GitHooks.Domain.Ast.Visitors;
using GitHooks.Domain.Common;

namespace GitHooks.Domain.Ast;

// TODO: Evaluate whether record semantics are actually needed.
// AST nodes may be better represented as classes if value equality
// and with-expressions are not used.
public abstract record AstNode
{
    public abstract AstNodeKind Kind { get; }

    public required SourceSpan Span { get; init; }

    public abstract void Accept(
        IAstCommandVisitor visitor);

    public abstract TResult Accept<TResult>(
        IAstQueryVisitor<TResult> visitor);
}
