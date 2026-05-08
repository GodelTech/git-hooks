using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record PipelineNode(
    IReadOnlyList<ParameterNode> Parameters,
    IReadOnlyList<StepNode> Steps,
    IReadOnlyList<UnknownFieldNode> UnknownFields,
    SourceSpan Span)
    : AstNode(UnknownFields, Span);
