namespace GitHooks.Infrastructure;

/// <summary>
/// Defines file-system operations for creating git hook files.
/// </summary>
public interface IHookFileManager
{
    /// <summary>
    /// Creates hook files in the given directory.
    /// </summary>
    /// <param name="hooksPath">The target directory for hook files.</param>
    /// <param name="hookNames">The hook file names to create.</param>
    /// <param name="overwrite">A value indicating whether existing files should be overwritten.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning creation details.</returns>
    public Task<HookFileCreationResult> CreateHookFilesAsync(string hooksPath, IReadOnlyCollection<string> hookNames, bool overwrite, CancellationToken cancellationToken = default);
}

