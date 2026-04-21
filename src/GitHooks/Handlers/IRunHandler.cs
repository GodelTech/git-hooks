namespace GitHooks.Handlers;

/// <summary>
/// Defines a handler for reading and displaying a hook YAML file.
/// </summary>
public interface IRunHandler
{
    /// <summary>
    /// Reads and prints the provided YAML file along with repository path information.
    /// </summary>
    /// <param name="filePath">Path to the YAML file.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning an exit code.</returns>
    public Task<int> HandleAsync(string filePath, CancellationToken cancellationToken = default);
}
