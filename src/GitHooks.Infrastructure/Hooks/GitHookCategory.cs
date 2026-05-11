namespace GitHooks.Infrastructure.Hooks;

/// <summary>
/// Represents the Git lifecycle category that a hook belongs to.
/// </summary>
public enum GitHookCategory
{
    /// <summary>
    /// Hooks that fire during the commit workflow
    /// (<c>pre-commit</c>, <c>prepare-commit-msg</c>, <c>commit-msg</c>, <c>post-commit</c>).
    /// </summary>
    Commit,

    /// <summary>
    /// Hooks that fire during the push workflow (<c>pre-push</c>).
    /// </summary>
    Push,

    /// <summary>
    /// Hooks that fire during merge operations (<c>pre-merge-commit</c>, <c>post-merge</c>).
    /// </summary>
    Merge,

    /// <summary>
    /// Hooks that fire during rebase operations (<c>pre-rebase</c>, <c>post-rewrite</c>).
    /// </summary>
    Rebase,

    /// <summary>
    /// Hooks that fire after checkout or index changes
    /// (<c>post-checkout</c>, <c>post-index-change</c>).
    /// </summary>
    Checkout,
}

