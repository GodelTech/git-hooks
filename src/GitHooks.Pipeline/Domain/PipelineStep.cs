namespace GitHooks.Pipeline.Domain;

/// <summary>
/// Represents a validated pipeline step.
/// </summary>
/// <param name="Kind">The step kind.</param>
/// <param name="Command">The command payload: task name, script text, or pwsh text.</param>
/// <param name="Name">The optional step name.</param>
/// <param name="DisplayName">The optional display name.</param>
/// <param name="Enabled">The optional enabled flag as a raw string.</param>
/// <param name="ContinueOnError">The optional continueOnError flag as a raw string.</param>
/// <param name="Inputs">Optional step inputs as raw string values.</param>
public sealed record PipelineStep(
    PipelineStepKind Kind,
    string Command,
    string? Name,
    string? DisplayName,
    string? Enabled,
    string? ContinueOnError,
    IReadOnlyDictionary<string, string?> Inputs);
