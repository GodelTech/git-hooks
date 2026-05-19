using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Resolution;

/// <summary>
/// Resolves a pipeline file into a bound and template-expanded pipeline.
/// </summary>
public interface IPipelineResolver
{
    /// <summary>
    /// Reads, compiles, and expands the pipeline at the supplied file path.
    /// </summary>
    /// <param name="filePath">Path to the root pipeline file.</param>
    /// <param name="parameterOverrides">Optional parameter overrides applied during compilation.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous file operations.</param>
    /// <returns>A fully resolved pipeline.</returns>
    public Task<PipelineNode> ResolveAsync(
        string filePath,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default);
}
