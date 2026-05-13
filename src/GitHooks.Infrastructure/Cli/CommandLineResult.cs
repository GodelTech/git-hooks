namespace GitHooks.Infrastructure.Cli;

/// <summary>
/// Represents the result of a command-line process execution.
/// </summary>
public sealed class CommandLineResult
{
    private CommandLineResult(string output, string error, int exitCode)
    {
        IsSuccess = exitCode == 0;
        Output = output;
        Error = error;
        ExitCode = exitCode;
    }

    /// <summary>
    /// Gets a value indicating whether the process exited successfully (exit code 0).
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the standard output of the process.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Gets the standard error output of the process.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets the exit code of the process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Creates a <see cref="CommandLineResult"/> from a completed process's pre-read output.
    /// Streams must be read asynchronously before calling this method to avoid pipe buffer deadlocks.
    /// </summary>
    /// <param name="output">The standard output text.</param>
    /// <param name="error">The standard error text.</param>
    /// <param name="exitCode">The process exit code.</param>
    /// <returns>A new <see cref="CommandLineResult"/>.</returns>
    public static CommandLineResult FromProcess(string output, string error, int exitCode)
    {
        return new CommandLineResult(output, error, exitCode);
    }

    /// <summary>
    /// Creates a failed <see cref="CommandLineResult"/> from an exception.
    /// </summary>
    /// <param name="ex">The exception that occurred during process execution.</param>
    /// <returns>A new <see cref="CommandLineResult"/> with exit code 1 and the exception message as the error.</returns>
    public static CommandLineResult FromException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return new CommandLineResult(string.Empty, ex.Message, 1);
    }
}
