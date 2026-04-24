using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Contracts;

/// <summary>
/// Previews parsed pipeline steps in execution order.
/// </summary>
public interface IStepPreviewRunner
{
    /// <summary>
    /// Displays a preview of pipeline steps and returns an exit code.
    /// </summary>
    /// <param name="pipeline">The parsed pipeline definition.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation with an exit code.</returns>
    public Task<int> PreviewAsync(PipelineDefinition pipeline, CancellationToken cancellationToken = default);
}
