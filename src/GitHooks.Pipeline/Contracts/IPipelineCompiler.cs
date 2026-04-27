namespace GitHooks.Pipeline.Contracts;

/// <summary>
/// Compiles a parsed pipeline definition into an executable plan.
/// </summary>
public interface IPipelineCompiler
{
    /// <summary>
    /// Compiles the supplied pipeline request.
    /// </summary>
    /// <param name="request">The compile input request.</param>
    /// <returns>A <see cref="CompileResult"/> that contains a plan or compile errors.</returns>
    public CompileResult Compile(CompileRequest request);
}
