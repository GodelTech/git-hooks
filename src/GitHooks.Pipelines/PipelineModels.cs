namespace GitHooks.Pipelines;

/// <summary>
/// Represents a parsed hook pipeline definition.
/// </summary>
/// <param name="Parameters">Declared pipeline parameters.</param>
/// <param name="Steps">Declared pipeline steps.</param>
public sealed record HookPipelineDefinition(
    IReadOnlyList<PipelineParameterDefinition> Parameters,
    IReadOnlyList<PipelineStepDefinition> Steps);

/// <summary>
/// Represents a pipeline parameter definition.
/// </summary>
/// <param name="Name">The parameter name.</param>
/// <param name="Type">The parameter type as declared in YAML.</param>
/// <param name="DisplayName">The friendly parameter label.</param>
/// <param name="DefaultValue">The optional default value as raw string.</param>
/// <param name="Values">Optional allowed values as raw strings.</param>
public sealed record PipelineParameterDefinition(
    string Name,
    string Type,
    string? DisplayName,
    string? DefaultValue,
    IReadOnlyList<string?> Values);

/// <summary>
/// Enumerates supported step kinds.
/// </summary>
public enum PipelineStepKind
{
    /// <summary>
    /// A generic task step.
    /// </summary>
    Task,

    /// <summary>
    /// An inline shell script step.
    /// </summary>
    Script,

    /// <summary>
    /// An inline PowerShell step.
    /// </summary>
    Pwsh,
}

/// <summary>
/// Represents a single parsed pipeline step.
/// </summary>
/// <param name="Kind">The step kind.</param>
/// <param name="Command">The command payload (task name, script text, or pwsh text).</param>
/// <param name="Name">The optional step name.</param>
/// <param name="DisplayName">The optional display name.</param>
/// <param name="Enabled">The optional enabled value as raw string.</param>
/// <param name="ContinueOnError">The optional continueOnError value as raw string.</param>
/// <param name="Inputs">Optional step inputs as raw string values.</param>
public sealed record PipelineStepDefinition(
    PipelineStepKind Kind,
    string Command,
    string? Name,
    string? DisplayName,
    string? Enabled,
    string? ContinueOnError,
    IReadOnlyDictionary<string, string?> Inputs);
