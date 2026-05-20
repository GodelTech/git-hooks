using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.IO;

/// <summary>
/// Reads raw pipeline content from a source.
/// </summary>
public interface IPipelineContentReader
{
    /// <summary>
    /// Reads all text from the specified source.
    /// </summary>
    /// <param name="source">Pipeline source to read.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
    /// <returns>The source content.</returns>
    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default);
}
