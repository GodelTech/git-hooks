namespace GitHooks.Infrastructure;

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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="scope"/> is not a defined enum value.</exception>
    public static string ToGitString(this GitConfigScope scope)
    {
        // Switch expression ensures a compile-time warning if a new enum member is added without updating this mapping.
        return scope switch
        {
            GitConfigScope.Local => "local",
            GitConfigScope.Global => "global",
            GitConfigScope.System => "system",
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, $"Unexpected {nameof(GitConfigScope)} value.")
        };
    }
}

