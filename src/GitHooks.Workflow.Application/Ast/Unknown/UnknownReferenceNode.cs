using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast.Unknown;

public sealed record UnknownReferenceNode(string Value, SourceSpan Span)
    : UnknownNode(Span);
