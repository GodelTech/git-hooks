namespace GitHooks.Pipeline.Execution;

/// <summary>
/// Represents the outcome of pipeline execution.
/// </summary>
/// <param name="IsSuccess">Whether all steps completed successfully.</param>
/// <param name="FailedStepIndex">The 1-based failed step index when unsuccessful.</param>
/// <param name="ErrorMessage">The associated error message when unsuccessful.</param>
public sealed record PipelineRunResult(bool IsSuccess, int? FailedStepIndex, string? ErrorMessage)
{
    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static PipelineRunResult Success()
    {
        return new PipelineRunResult(true, null, null);
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="failedStepIndex">The failed step index.</param>
    /// <param name="errorMessage">The failure message.</param>
    /// <returns>A failed result instance.</returns>
    public static PipelineRunResult Failure(int failedStepIndex, string errorMessage)
    {
        return new PipelineRunResult(false, failedStepIndex, errorMessage);
    }
}
