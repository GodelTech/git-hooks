using System.Diagnostics;

namespace GitHooks.CommandLine;

/// <summary>
/// Represents the result of a command-line process execution.
/// </summary>
public class CommandLineResult
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
    /// Creates a <see cref="CommandLineResult"/> from a completed <see cref="Process"/>.
    /// </summary>
    /// <param name="process">The process that has finished execution.</param>
    /// <returns>A new <see cref="CommandLineResult"/> containing the process output, error, and exit code.</returns>
    public static CommandLineResult FromProcess(Process process)
    {
        return new CommandLineResult(
            process.StandardOutput.ReadToEnd(),
            process.StandardError.ReadToEnd(),
            process.ExitCode
        );
    }

    /// <summary>
    /// Creates a failed <see cref="CommandLineResult"/> from an exception.
    /// </summary>
    /// <param name="ex">The exception that occurred during process execution.</param>
    /// <returns>A new <see cref="CommandLineResult"/> with exit code 1 and the exception message as the error.</returns>
    public static CommandLineResult FromException(Exception ex)
    {
        return new CommandLineResult(string.Empty, ex.Message, 1);
    }
}
