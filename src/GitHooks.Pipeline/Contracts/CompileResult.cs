using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Contracts;

/// <summary>
/// Represents the result of compiling a parsed pipeline.
/// </summary>
public sealed class CompileResult
{
    private CompileResult(PipelineExecutionPlan plan)
    {
        IsSuccess = true;
        Plan = plan;
        Errors = [];
    }

    private CompileResult(IReadOnlyList<ParseError> errors)
    {
        IsSuccess = false;
        Plan = null;
        Errors = errors;
    }

    /// <summary>Gets a value indicating whether compilation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets the executable plan when <see cref="IsSuccess"/> is <see langword="true"/>.</summary>
    public PipelineExecutionPlan? Plan { get; }

    /// <summary>Gets the list of compile errors when <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    public IReadOnlyList<ParseError> Errors { get; }

    /// <summary>
    /// Creates a successful compile result.
    /// </summary>
    /// <param name="plan">The executable pipeline plan.</param>
    /// <returns>A successful <see cref="CompileResult"/>.</returns>
    public static CompileResult Ok(PipelineExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new(plan);
    }

    /// <summary>
    /// Creates a failed compile result.
    /// </summary>
    /// <param name="errors">The list of compile errors.</param>
    /// <returns>A failed <see cref="CompileResult"/>.</returns>
    public static CompileResult Fail(IReadOnlyList<ParseError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new(errors);
    }
}
