namespace GitHooks.Pipelines;

/// <summary>
/// Represents a parser diagnostic item.
/// </summary>
/// <param name="Path">The YAML path where the issue was detected.</param>
/// <param name="Message">The diagnostic message.</param>
public sealed record PipelineParseDiagnostic(string Path, string Message);

/// <summary>
/// Represents the result of YAML parsing.
/// </summary>
/// <param name="IsSuccess">Indicates whether parsing succeeded.</param>
/// <param name="Pipeline">The parsed pipeline model when successful.</param>
/// <param name="Diagnostics">Validation or parsing diagnostics.</param>
public sealed record PipelineParseResult(
    bool IsSuccess,
    HookPipelineDefinition? Pipeline,
    IReadOnlyList<PipelineParseDiagnostic> Diagnostics)
{
    /// <summary>
    /// Creates a successful parse result.
    /// </summary>
    /// <param name="pipeline">The parsed pipeline model.</param>
    /// <returns>A successful parse result.</returns>
    public static PipelineParseResult Success(HookPipelineDefinition pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        return new PipelineParseResult(
            IsSuccess: true,
            Pipeline: pipeline,
            Diagnostics: []);
    }

    /// <summary>
    /// Creates a failed parse result.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to return.</param>
    /// <returns>A failed parse result.</returns>
    public static PipelineParseResult Failure(IReadOnlyList<PipelineParseDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return new PipelineParseResult(
            IsSuccess: false,
            Pipeline: null,
            Diagnostics: diagnostics);
    }
}
