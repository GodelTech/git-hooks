namespace GitHooks.Pipeline.Domain.Model;

/// <summary>
/// Represents an inline script step.
/// </summary>
/// <param name="Id">The stable step identifier.</param>
/// <param name="Location">The source location where this step starts.</param>
/// <param name="Script">The script content to execute.</param>
public sealed record ScriptStep(StepId Id, SourceLocation Location, string Script)
    : Step(Id, Location);
