using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record ParameterNode(
    string Name,
    string? DisplayName,
    ParameterType Type,
    string? Default,
    IReadOnlyList<string> Values,
    SourceSpan Span
) : AstNode(Span);
