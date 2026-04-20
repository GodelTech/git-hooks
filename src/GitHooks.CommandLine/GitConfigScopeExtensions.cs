namespace GitHooks.CommandLine;

/// <summary>
/// Extension methods for <see cref="GitConfigScope"/>.
/// </summary>
public static class GitConfigScopeExtensions
{
    /// <summary>
    /// Converts a <see cref="GitConfigScope"/> value to its lowercase string representation used by git commands.
    /// </summary>
    /// <param name="scope">The git configuration scope.</param>
    /// <returns>The lowercase string representation of the scope (e.g., "local", "global", "system").</returns>
    public static string ToGitString(this GitConfigScope scope)
    {
        return scope.ToString().ToLowerInvariant();
    }
}
