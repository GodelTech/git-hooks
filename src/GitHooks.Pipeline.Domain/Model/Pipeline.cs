namespace GitHooks.Pipeline.Domain.Model;

/// <summary>
/// Represents a parsed pipeline.
/// </summary>
/// <param name="Location">The source location where the pipeline root starts.</param>
/// <param name="Steps">The ordered list of pipeline steps.</param>
public sealed record PipelineOld(SourceLocation Location, IReadOnlyList<Step> Steps);
