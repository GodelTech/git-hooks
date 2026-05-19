using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Expansion;

/// <summary>
/// Expands template steps in a pipeline into a flat step list.
/// </summary>
public interface IPipelineTemplateExpander
{
    /// <summary>
    /// Expands all template steps recursively and returns a pipeline whose steps are resolved.
    /// </summary>
    /// <param name="pipeline">The pipeline to expand.</param>
    /// <param name="pipelineFilePath">Absolute or relative path to the source YAML file.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous I/O operations.</param>
    /// <returns>A pipeline with template steps expanded.</returns>
    public Task<PipelineNode> ExpandAsync(
        PipelineNode pipeline,
        string pipelineFilePath,
        CancellationToken cancellationToken = default);
}
