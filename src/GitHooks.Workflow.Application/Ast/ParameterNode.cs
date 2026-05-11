using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record ParameterNode(
    string Name,
    string? DisplayName,
    ParameterType Type,
    string? Default,
    IReadOnlyList<string> Values,
    IReadOnlyList<UnknownFieldNode> UnknownFields,
    SourceSpan Span)
    : AstNode(UnknownFields, Span);
