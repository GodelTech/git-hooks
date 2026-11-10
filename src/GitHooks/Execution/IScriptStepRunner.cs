namespace GitHooks.Execution;

/// <summary>
/// Executes the script text of a single script step.
/// </summary>
public interface IScriptStepRunner
{
    /// <summary>
    /// Runs the given script in a shell process, streaming its output to the console.
    /// </summary>
    /// <param name="script">The fully substituted script text.</param>
    /// <param name="workingDirectory">The directory to run the script in.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The exit code of the shell process.</returns>
    public Task<int> RunAsync(string script, string workingDirectory, CancellationToken cancellationToken = default);
}
