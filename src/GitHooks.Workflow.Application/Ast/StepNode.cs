using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public abstract record StepNode(SourceSpan Span)
    : AstNode(Span)
{
    public string? DisplayName { get; init; }
}
