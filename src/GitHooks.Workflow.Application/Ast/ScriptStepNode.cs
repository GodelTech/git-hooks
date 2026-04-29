using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record ScriptStepNode(SourceSpan Span, string Script)
    : StepNode(Span);
