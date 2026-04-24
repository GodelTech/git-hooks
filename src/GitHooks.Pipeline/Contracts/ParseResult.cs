using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Contracts;

/// <summary>
/// Represents the result of YAML pipeline parsing.
/// </summary>
public sealed class ParseResult
{
    private ParseResult(PipelineDefinition pipeline)
    {
        IsSuccess = true;
        Pipeline = pipeline;
        Errors = [];
    }

    private ParseResult(IReadOnlyList<ParseError> errors)
    {
        IsSuccess = false;
        Pipeline = null;
        Errors = errors;
    }

    /// <summary>Gets a value indicating whether parsing succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets the parsed pipeline model when <see cref="IsSuccess"/> is <see langword="true"/>.</summary>
    public PipelineDefinition? Pipeline { get; }

    /// <summary>Gets the list of parse errors when <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    public IReadOnlyList<ParseError> Errors { get; }

    /// <summary>
    /// Creates a successful parse result.
    /// </summary>
    /// <param name="pipeline">The parsed pipeline definition.</param>
    /// <returns>A successful <see cref="ParseResult"/>.</returns>
    public static ParseResult Ok(PipelineDefinition pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        return new(pipeline);
    }

    /// <summary>
    /// Creates a failed parse result.
    /// </summary>
    /// <param name="errors">The list of parse errors.</param>
    /// <returns>A failed <see cref="ParseResult"/>.</returns>
    public static ParseResult Fail(IReadOnlyList<ParseError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new(errors);
    }
}
