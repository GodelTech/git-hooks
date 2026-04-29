using GitHooks.Pipeline.Domain.Execution;
using GitHooks.Pipeline.Domain.Model;

using PipelineModel = GitHooks.Pipeline.Domain.Model.PipelineOld;

namespace GitHooks.Pipeline.Execution;

/// <summary>
/// Executes an expanded pipeline AST.
/// </summary>
public interface IPipelineRunner
{
    /// <summary>
    /// Runs steps from the provided pipeline in order.
    /// </summary>
    /// <param name="pipeline">The expanded pipeline to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result describing the execution outcome.</returns>
    public Task<PipelineResult> RunAsync(PipelineModel pipeline, CancellationToken cancellationToken = default);
}
