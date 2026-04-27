using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Contracts;

/// <summary>
/// Processes parsed pipeline steps in execution order.
/// </summary>
public interface IStepRunner
{
    /// <summary>
    /// Runs step processing for the supplied pipeline and returns an exit code.
    /// </summary>
    /// <param name="plan">The compiled executable pipeline plan.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation with an exit code.</returns>
    public Task<int> RunAsync(PipelineExecutionPlan plan, CancellationToken cancellationToken = default);
}
