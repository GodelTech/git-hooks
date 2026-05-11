using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast.Unknown;

public sealed record UnknownMappingNode(
    IReadOnlyList<UnknownMappingEntryNode> Entries,
    SourceSpan Span
) : UnknownNode(Span);

