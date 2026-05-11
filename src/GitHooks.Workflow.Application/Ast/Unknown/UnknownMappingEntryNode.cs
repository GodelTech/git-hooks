using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast.Unknown;

public sealed record UnknownMappingEntryNode(
    UnknownNode Key,
    UnknownNode Value,
    SourceSpan Span
);

