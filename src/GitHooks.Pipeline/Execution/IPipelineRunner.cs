using GitHooks.Pipeline.Ast;

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
    public Task<PipelineRunResult> RunAsync(PipelineNode pipeline, CancellationToken cancellationToken = default);
}
