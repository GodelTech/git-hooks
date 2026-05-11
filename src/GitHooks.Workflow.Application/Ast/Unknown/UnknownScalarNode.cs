using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast.Unknown;

public sealed record UnknownScalarNode(string Value, SourceSpan Span)
    : UnknownNode(Span);

