using GitHooks.Pipeline.Domain.Model;

using PipelineModel = GitHooks.Pipeline.Domain.Model.PipelineOld;

namespace GitHooks.Pipeline.Transformation;

/// <summary>
/// Expands template steps into concrete executable steps.
/// </summary>
public interface ITemplateExpander
{
    /// <summary>
    /// Expands template steps in a pipeline recursively.
    /// </summary>
    /// <param name="pipeline">The pipeline AST to expand.</param>
    /// <param name="pipelineFilePath">The source file path used for relative template resolution.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new expanded pipeline.</returns>
    public Task<PipelineModel> ExpandAsync(PipelineModel pipeline, string pipelineFilePath, CancellationToken cancellationToken = default);
}
