namespace GitHooks.Pipeline.Domain.Model;

/// <summary>
/// Represents a template include step.
/// </summary>
/// <param name="Id">The stable step identifier.</param>
/// <param name="Location">The source location where this step starts.</param>
/// <param name="TemplatePath">The relative template file path.</param>
/// <param name="Parameters">Template parameters passed to the included template.</param>
public sealed record TemplateStep(
    StepId Id,
    SourceLocation Location,
    string TemplatePath,
    IReadOnlyDictionary<string, string> Parameters)
    : Step(Id, Location);
