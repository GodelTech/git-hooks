namespace GitHooks.Pipeline.Domain;

/// <summary>
/// Represents one executable pipeline step after compilation.
/// </summary>
/// <param name="Order">The 1-based execution order.</param>
/// <param name="Kind">The step kind.</param>
/// <param name="Command">The executable command payload.</param>
/// <param name="Name">The optional step name.</param>
/// <param name="DisplayName">The optional display name.</param>
/// <param name="Enabled">A value indicating whether this step is enabled.</param>
/// <param name="ContinueOnError">A value indicating whether this step can continue after failure.</param>
/// <param name="Inputs">Compiled input values for the step.</param>
public sealed record ExecutableStep(
    int Order,
    PipelineStepKind Kind,
    string Command,
    string? Name,
    string? DisplayName,
    bool Enabled,
    bool ContinueOnError,
    IReadOnlyDictionary<string, object?> Inputs);
