using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record PipelineNode(SourceSpan Span, IReadOnlyList<StepNode> Steps)
    : AstNode(Span);
