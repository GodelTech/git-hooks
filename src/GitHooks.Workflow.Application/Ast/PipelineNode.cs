using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record PipelineNode(IReadOnlyList<StepNode> Steps, SourceSpan Span)
    : AstNode(Span);
