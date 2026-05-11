using GitHooks.Infrastructure.Hooks;

namespace GitHooks.Infrastructure.Scaffold;

/// <summary>
/// Defines file-system operations for creating git hook scaffold files.
/// </summary>
public interface IGitHookScaffolder
{
    /// <summary>
    /// Creates hook scaffold files in the given directory, resolved against the repository root.
    /// </summary>
    /// <param name="repositoryRootPath">The absolute path to the repository root, used to resolve relative <paramref name="hooksPath"/> values.</param>
    /// <param name="hooksPath">The target directory for hook scaffold files. Relative paths are resolved against <paramref name="repositoryRootPath"/>.</param>
    /// <param name="hooks">The hooks to create scaffold files for, providing both names and metadata.</param>
    /// <param name="overwrite">A value indicating whether existing files should be overwritten.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning scaffold creation details.</returns>
    public Task<GitHookScaffoldResult> CreateScaffoldAsync(string repositoryRootPath, string hooksPath, IReadOnlyCollection<GitHook> hooks, bool overwrite, CancellationToken cancellationToken = default);
}

