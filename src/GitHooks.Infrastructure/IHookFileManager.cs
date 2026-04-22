namespace GitHooks.Infrastructure;

/// <summary>
/// Defines file-system operations for creating git hook files.
/// </summary>
public interface IHookFileManager
{
    /// <summary>
    /// Creates hook files in the given directory, resolved against the repository root.
    /// </summary>
    /// <param name="repositoryRootPath">The absolute path to the repository root, used to resolve relative <paramref name="hooksPath"/> values.</param>
    /// <param name="hooksPath">The target directory for hook files. Relative paths are resolved against <paramref name="repositoryRootPath"/>.</param>
    /// <param name="hookNames">The hook file names to create.</param>
    /// <param name="overwrite">A value indicating whether existing files should be overwritten.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning creation details.</returns>
    public Task<HookFileCreationResult> CreateHookFilesAsync(string repositoryRootPath, string hooksPath, IReadOnlyCollection<string> hookNames, bool overwrite, CancellationToken cancellationToken = default);
}
