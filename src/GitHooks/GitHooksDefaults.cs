using GitHooks.Infrastructure;

namespace GitHooks;

/// <summary>
/// Default values used across commands and handlers in this tool.
/// </summary>
public static class GitHooksDefaults
{
    /// <summary>The default Git configuration scope.</summary>
    public const GitConfigScope Scope = GitConfigScope.Global;

    /// <summary>The default directory where hook files are stored.</summary>
    public const string HooksPath = ".githooks";
}

