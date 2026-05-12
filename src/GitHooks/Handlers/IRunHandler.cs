namespace GitHooks.Handlers;

/// <summary>
/// Defines a handler for parsing and validating a hook YAML file.
/// </summary>
public interface IRunHandler
{
    /// <summary>
    /// Parses and validates the provided YAML file along with repository path information.
    /// </summary>
    /// <param name="filePath">Path to the YAML file.</param>
    /// <param name="parameterOverrides">Optional command-line parameter values that override YAML defaults.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning an exit code.</returns>
    public Task<int> HandleAsync(
        string filePath,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default);
}
