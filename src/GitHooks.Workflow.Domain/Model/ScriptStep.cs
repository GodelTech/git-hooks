namespace GitHooks.Workflow.Domain.Model;

public sealed record ScriptStep(StepId Id, SourceSpan Span, string Script)
    : StepBase(Id, Span);

