namespace GitHooks.Pipelines.Domain;

/// <summary>
/// Enumerates supported step kinds.
/// </summary>
public enum PipelineStepKind
{
    /// <summary>
    /// A generic named task step.
    /// </summary>
    Task,

    /// <summary>
    /// An inline shell script step.
    /// </summary>
    Script,

    /// <summary>
    /// An inline PowerShell step.
    /// </summary>
    Pwsh,
}
