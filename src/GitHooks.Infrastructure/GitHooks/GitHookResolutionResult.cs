namespace GitHooks.Infrastructure.GitHooks;

/// <summary>
/// Represents the outcome of validating and normalizing user-provided hook names.
/// </summary>
/// <param name="SupportedHooks">
/// The normalized and resolved hooks that exist in the catalog.
/// </param>
/// <param name="InvalidHooks">
/// The normalized hook names that are not recognized by the catalog.
/// </param>
public sealed record GitHookResolutionResult(
    IReadOnlyCollection<GitHook> SupportedHooks,
    IReadOnlyCollection<string> InvalidHooks)
{
    /// <summary>
    /// Gets a value indicating whether all provided hook names are supported.
    /// </summary>
    public bool IsValid => InvalidHooks.Count == 0;
}
