using GitHooks.Infrastructure.Git;

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

    /// <summary>Default commit hook names.</summary>
    public static readonly string[] CommitHooks =
    [
        "pre-commit",
        "prepare-commit-msg",
        "commit-msg",
        "post-commit"
    ];

    /// <summary>Default push hook names.</summary>
    public static readonly string[] PushHooks =
    [
        "pre-push"
    ];
}
