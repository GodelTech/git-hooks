namespace GitHooks.Handlers;

/// <summary>
/// TEMPORARY: manual verification handler for <see cref="Compilation.PipelineCompiler"/>.
/// Not intended to ship long-term; used to visually confirm the
/// YAML -> Parser -> Domain AST -> Template Expansion -> Validation flow.
/// </summary>
public interface IRunPipelineCompilerHandler
{
    /// <summary>
    /// Compiles the provided YAML file using the real <see cref="Compilation.PipelineCompiler"/>
    /// pipeline and prints the resulting AST summary and diagnostics.
    /// </summary>
    /// <param name="filePath">Path to the YAML file.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning an exit code.</returns>
    public Task<int> HandleAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
