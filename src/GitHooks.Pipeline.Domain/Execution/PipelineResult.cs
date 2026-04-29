namespace GitHooks.Pipeline.Domain.Execution;

/// <summary>
/// Represents the outcome of pipeline execution.
/// </summary>
/// <param name="IsSuccess">Whether all steps completed successfully.</param>
/// <param name="FailedStepId">The failed step identifier when unsuccessful.</param>
/// <param name="ErrorMessage">The associated error message when unsuccessful.</param>
public sealed record PipelineResult(bool IsSuccess, int? FailedStepId, string? ErrorMessage)
{
    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static PipelineResult Success()
    {
        return new PipelineResult(true, null, null);
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="failedStepId">The failed step identifier.</param>
    /// <param name="errorMessage">The failure message.</param>
    /// <returns>A failed result instance.</returns>
    public static PipelineResult Failure(int failedStepId, string errorMessage)
    {
        return new PipelineResult(false, failedStepId, errorMessage);
    }
}
