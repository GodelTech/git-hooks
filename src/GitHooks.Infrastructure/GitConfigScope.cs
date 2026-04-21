namespace GitHooks.Infrastructure;

/// <summary>
/// Specifies the git configuration scope for reading and writing settings.
/// </summary>
public enum GitConfigScope
{
    /// <summary>
    /// Repository-level configuration (.git/config). Applies to the current repository only.
    /// </summary>
    Local,

    /// <summary>
    /// User-level configuration (~/.gitconfig). Applies to all repositories for the current user.
    /// </summary>
    Global,

    /// <summary>
    /// Machine-level configuration. Applies to all users on the system. Requires elevated permissions.
    /// </summary>
    System
}

