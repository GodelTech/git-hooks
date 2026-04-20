namespace GitHooks.CommandLine;

/// <summary>
/// Defines a generic command-line runner.
/// </summary>
public interface ICommandLineRunner
{
    /// <summary>
    /// Runs a command-line process asynchronously.
    /// </summary>
    /// <param name="fileName">The name or path of the executable to run.</param>
    /// <param name="arguments">The arguments to pass to the executable.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning a <see cref="CommandLineResult"/> with the process outcome.</returns>
    public Task<CommandLineResult> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default);
}
