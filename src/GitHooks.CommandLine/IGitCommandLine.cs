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

    /// <summary>
    /// Gets the core.hooksPath configuration value.
    /// </summary>
    /// <param name="scope">The configuration scope ("global" or "system").</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning the command-line result.</returns>
    public Task<CommandLineResult> GetCoreHooksPathAsync(string scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the core.hooksPath configuration value.
    /// </summary>
    /// <param name="scope">The configuration scope ("global" or "system").</param>
    /// <param name="value">The value to set.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning the command-line result.</returns>
    public Task<CommandLineResult> SetCoreHooksPathAsync(string scope, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsets the core.hooksPath configuration value.
    /// </summary>
    /// <param name="scope">The configuration scope ("global" or "system").</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning the command-line result.</returns>
    public Task<CommandLineResult> UnsetCoreHooksPathAsync(string scope, CancellationToken cancellationToken = default);
}
