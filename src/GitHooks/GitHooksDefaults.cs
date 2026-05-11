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

    /// <summary>The default commit hooks created when no hooks are specified.</summary>
    public static readonly string[] CommitHooks = ["pre-commit", "prepare-commit-msg", "commit-msg", "post-commit"];

    /// <summary>The default push hooks created when no hooks are specified.</summary>
    public static readonly string[] PushHooks = ["pre-push"];

    /// <summary>The default hooks created by the create command when no hooks are specified.</summary>
    public static readonly string[] CreateHooks = [.. CommitHooks, .. PushHooks];
}

