namespace GitHooks.Handlers;

/// <summary>
/// Defines a handler for uninstalling git hooks.
/// </summary>
public interface IUninstallHandler
{
    /// <summary>
    /// Handles the uninstallation of git hooks by removing the core.hooksPath configuration.
    /// </summary>
    /// <param name="scope">The git configuration scope ("global" or "system").</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning an exit code.</returns>
    public Task<int> HandleAsync(string scope, CancellationToken cancellationToken = default);
}
