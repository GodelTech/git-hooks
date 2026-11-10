namespace GitHooks.Execution;

/// <summary>
/// Defines a handler for compiling and executing a hook YAML file.
/// </summary>
public interface IRunHandler
{
    /// <summary>
    /// Compiles the provided YAML file and executes its steps from the repository root.
    /// </summary>
    /// <param name="filePath">Path to the YAML file.</param>
    /// <param name="parameterOverrides">Optional command-line parameter values that override YAML defaults.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task returning an exit code defined by <see cref="RunExitCode"/>.</returns>
    public Task<int> HandleAsync(
        string filePath,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default);
}
