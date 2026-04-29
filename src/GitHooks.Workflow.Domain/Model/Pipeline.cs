namespace GitHooks.Workflow.Domain.Model;

public sealed record Pipeline(SourceSpan Span, IReadOnlyList<StepBase> Steps);
