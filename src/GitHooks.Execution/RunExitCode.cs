namespace GitHooks.Execution;

/// <summary>
/// Defines the run process exit-code contract.
/// </summary>
public static class RunExitCode
{
    /// <summary>
    /// Indicates all steps completed successfully.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Indicates infrastructure, input, compilation, or execution-plan validation failed.
    /// </summary>
    public const int CompilationFailed = 1;

    /// <summary>
    /// Indicates a step ran and returned a nonzero exit code.
    /// </summary>
    public const int StepFailed = 2;

    /// <summary>
    /// Indicates a step could not be run.
    /// </summary>
    public const int ExecutionError = 3;
}
