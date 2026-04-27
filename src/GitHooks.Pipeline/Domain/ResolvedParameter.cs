namespace GitHooks.Pipeline.Domain;

/// <summary>
/// Represents one resolved and typed parameter value in the executable plan.
/// </summary>
/// <param name="Name">The parameter name.</param>
/// <param name="Type">The declared parameter type.</param>
/// <param name="Value">The resolved parameter value.</param>
/// <param name="IsDefaulted">A value indicating whether the default value was used.</param>
public sealed record ResolvedParameter(
    string Name,
    string Type,
    object? Value,
    bool IsDefaulted);
