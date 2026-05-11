using GitHooks.Infrastructure.Git;

namespace GitHooks.Handlers;

/// <summary>
/// Defines a handler for creating git hook scaffold files.
/// </summary>
public interface ICreateHookHandler
{
    /// <summary>
    /// Creates git hook scaffold files in the configured hooks directory.
    /// </summary>
    /// <param name="scope">The git configuration scope used for repository validation.</param>
    /// <param name="hooksPath">The directory where hook scaffold files should be created.</param>
    /// <param name="hooks">The hook names to create.</param>
    /// <param name="force">A value indicating whether existing files should be overwritten.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning an exit code.</returns>
    public Task<int> HandleAsync(GitConfigScope scope, string hooksPath, IReadOnlyCollection<string> hooks, bool force, CancellationToken cancellationToken = default);
}

