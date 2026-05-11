namespace GitHooks.Templates;

/// <summary>
/// Resolves the current tool version used in generated scaffold content.
/// </summary>
public interface IToolVersionResolver
{
    /// <summary>
    /// Resolves the tool version string.
    /// </summary>
    /// <returns>The resolved version value.</returns>
    public string ResolveVersion();
}
