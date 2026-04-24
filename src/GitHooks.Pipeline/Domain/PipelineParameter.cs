namespace GitHooks.Pipeline.Domain;

/// <summary>
/// Represents a validated pipeline parameter.
/// </summary>
/// <param name="Name">The parameter name.</param>
/// <param name="Type">The parameter type as declared in YAML.</param>
/// <param name="DisplayName">The optional friendly label.</param>
/// <param name="DefaultValue">The optional default value as a raw string.</param>
/// <param name="Values">Optional allowed values as raw strings.</param>
public sealed record PipelineParameter(
    string Name,
    string Type,
    string? DisplayName,
    string? DefaultValue,
    IReadOnlyList<string?> Values);
