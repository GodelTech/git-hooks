namespace GitHooks.Infrastructure.GitHooks;

/// <summary>
/// Typed convenience extension methods for <see cref="IGitHookCatalog"/>.
/// </summary>
/// <remarks>
/// These helpers exist as extensions rather than interface members so that
/// <see cref="IGitHookCatalog"/> remains minimal and does not need to be updated
/// when new <see cref="GitHookCategory"/> values are added.
/// </remarks>
public static class GitHookCatalogExtensions
{
    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Commit"/> category.
    /// </summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <returns>
    /// <c>pre-commit</c>, <c>prepare-commit-msg</c>, <c>commit-msg</c>, <c>post-commit</c>.
    /// </returns>
    public static IReadOnlyCollection<GitHook> GetCommitHooks(this IGitHookCatalog catalog)
    {
        return catalog.GetHooks(GitHookCategory.Commit);
    }

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Push"/> category.
    /// </summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <returns><c>pre-push</c>.</returns>
    public static IReadOnlyCollection<GitHook> GetPushHooks(this IGitHookCatalog catalog)
    {
        return catalog.GetHooks(GitHookCategory.Push);
    }

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Merge"/> category.
    /// </summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <returns><c>pre-merge-commit</c>, <c>post-merge</c>.</returns>
    public static IReadOnlyCollection<GitHook> GetMergeHooks(this IGitHookCatalog catalog)
    {
        return catalog.GetHooks(GitHookCategory.Merge);
    }

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Rebase"/> category.
    /// </summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <returns><c>pre-rebase</c>, <c>post-rewrite</c>.</returns>
    public static IReadOnlyCollection<GitHook> GetRebaseHooks(this IGitHookCatalog catalog)
    {
        return catalog.GetHooks(GitHookCategory.Rebase);
    }

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Checkout"/> category.
    /// </summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <returns><c>post-checkout</c>, <c>post-index-change</c>.</returns>
    public static IReadOnlyCollection<GitHook> GetCheckoutHooks(this IGitHookCatalog catalog)
    {
        return catalog.GetHooks(GitHookCategory.Checkout);
    }
}
