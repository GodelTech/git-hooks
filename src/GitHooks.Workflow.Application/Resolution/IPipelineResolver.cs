using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Resolution;

/// <summary>
/// Resolves a pipeline file into a bound and template-expanded pipeline.
/// </summary>
public interface IPipelineResolver
{
    /// <summary>
    /// Reads, compiles, and expands the pipeline at the supplied source.
    /// </summary>
    /// <param name="source">The canonical source identifying the root pipeline.</param>
    /// <param name="parameterOverrides">Optional parameter overrides applied during compilation.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous file operations.</param>
    /// <returns>A fully resolved pipeline.</returns>
    public Task<PipelineNode> ResolveAsync(
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default);
}
