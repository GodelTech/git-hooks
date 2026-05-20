using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

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
    /// <param name="source">The canonical source of the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous I/O operations.</param>
    /// <returns>A pipeline with template steps expanded.</returns>
    public Task<PipelineNode> ExpandAsync(
        PipelineNode pipeline,
        PipelineSource source,
        CancellationToken cancellationToken = default);
}
