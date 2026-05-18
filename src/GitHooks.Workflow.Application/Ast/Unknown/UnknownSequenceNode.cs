using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast.Unknown;

public sealed record UnknownSequenceNode(
    IReadOnlyList<UnknownNode> Items,
    SourceSpan Span)
    : UnknownNode(Span);
