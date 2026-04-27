namespace GitHooks.Pipeline.Domain;

/// <summary>
/// Represents a fully compiled executable pipeline plan.
/// </summary>
/// <param name="Parameters">Resolved parameter values indexed by name.</param>
/// <param name="Steps">Compiled executable steps in run order.</param>
public sealed record PipelineExecutionPlan(
    IReadOnlyDictionary<string, ResolvedParameter> Parameters,
    IReadOnlyList<ExecutableStep> Steps);
