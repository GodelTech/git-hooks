namespace GitHooks.Handlers;

/// <summary>
/// Defines a handler for installing git hooks.
/// </summary>
public interface IInstallHandler
{
    /// <summary>
    /// Handles the installation of git hooks.
    /// </summary>
    /// <param name="scope">The git configuration scope ("global" or "system").</param>
    /// <param name="hooksPath">The path where git hooks will be installed.</param>
    /// <param name="force">A value indicating whether to force installation, overwriting existing hooks.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning an exit code.</returns>
    public Task<int> HandleAsync(string scope, string hooksPath, bool force, CancellationToken cancellationToken = default);
}
