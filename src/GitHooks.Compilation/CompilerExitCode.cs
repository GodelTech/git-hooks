namespace GitHooks.Compilation;

/// <summary>
/// Defines the compiler process exit-code contract.
/// </summary>
public static class CompilerExitCode
{
    /// <summary>
    /// Indicates successful compilation with no error diagnostics.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Indicates compilation completed with one or more error diagnostics.
    /// </summary>
    public const int CompilationError = 1;

    /// <summary>
    /// Maps a compilation result to an exit code.
    /// Returns <see cref="CompilationError"/> when the result has errors; otherwise returns <see cref="Success"/>.
    /// </summary>
    /// <param name="result">The compilation result to map.</param>
    /// <returns>The exit code defined by this contract.</returns>
    public static int FromResult(
        CompilationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.HasErrors
            ? CompilationError
            : Success;
    }
}
