namespace GitHooks.CommandLine;

/// <summary>
/// Defines a command-line wrapper for Git operations.
/// </summary>
public interface IGitCommandLine
{
    /// <summary>
    /// Checks whether the git executable is available on the system.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning <c>true</c> if git is available; otherwise, <c>false</c>.</returns>
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
