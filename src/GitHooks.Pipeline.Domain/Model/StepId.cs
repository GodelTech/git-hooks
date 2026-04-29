namespace GitHooks.Pipeline.Domain.Model;

/// <summary>
/// Represents a stable 1-based step identifier inside a pipeline.
/// </summary>
/// <param name="Value">The numeric identifier value.</param>
public readonly record struct StepId(int Value)
{
    /// <summary>
    /// Creates a validated <see cref="StepId"/> value.
    /// </summary>
    /// <param name="value">The 1-based step index.</param>
    /// <returns>A validated step identifier.</returns>
    public static StepId FromIndex(int value)
    {
        if (value <= 0)
        {
            throw new PipelineException("Step id must be greater than zero.");
        }

        return new StepId(value);
    }
}
