using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast.Unknown;

public sealed record UnknownFieldNode(
    UnknownNode Key,
    UnknownNode Value,
    SourceSpan Span
);

