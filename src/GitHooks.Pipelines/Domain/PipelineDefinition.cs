namespace GitHooks.Pipelines.Domain;

/// <summary>
/// Represents a fully parsed and validated hook pipeline.
/// </summary>
/// <param name="Parameters">Declared pipeline parameters.</param>
/// <param name="Steps">Declared pipeline steps.</param>
public sealed record PipelineDefinition(
    IReadOnlyList<PipelineParameter> Parameters,
    IReadOnlyList<PipelineStep> Steps);
