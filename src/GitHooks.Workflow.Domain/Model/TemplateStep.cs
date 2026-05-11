namespace GitHooks.Workflow.Domain.Model;

public sealed record TemplateStep(
    StepId Id,
    SourceSpan Span,
    string TemplatePath,
    IReadOnlyDictionary<string, string> Parameters)
    : StepBase(Id, Span);

