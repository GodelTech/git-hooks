namespace GitHooks.Infrastructure.GitHooks;

/// <summary>
/// Provides a read-only catalog of well-known Git hooks grouped by lifecycle category.
/// </summary>
/// <remarks>
/// Inject this interface wherever Git hook metadata is needed. Use
/// <see cref="GetHooks(GitHookCategory)"/> as the primary programmatic entry point,
/// or call the typed convenience methods when the category is known at compile time.
/// </remarks>
public interface IGitHookCatalog
{
    /// <summary>
    /// Returns all hooks across every supported category.
    /// </summary>
    /// <returns>A read-only collection of all known <see cref="GitHook"/> entries.</returns>
    public IReadOnlyCollection<GitHook> GetAllHooks();

    /// <summary>
    /// Returns all hooks that belong to the specified <paramref name="category"/>.
    /// </summary>
    /// <param name="category">The lifecycle category to filter by.</param>
    /// <returns>
    /// A read-only collection of <see cref="GitHook"/> entries for the given category.
    /// Returns an empty collection if the category has no registered hooks.
    /// </returns>
    /// <example>
    /// <code>
    /// IReadOnlyCollection&lt;GitHook&gt; hooks = catalog.GetHooks(GitHookCategory.Commit);
    /// </code>
    /// </example>
    public IReadOnlyCollection<GitHook> GetHooks(GitHookCategory category);

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Commit"/> category.
    /// </summary>
    /// <returns>
    /// <c>pre-commit</c>, <c>prepare-commit-msg</c>, <c>commit-msg</c>, <c>post-commit</c>.
    /// </returns>
    public IReadOnlyCollection<GitHook> GetCommitHooks();

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Push"/> category.
    /// </summary>
    /// <returns><c>pre-push</c>.</returns>
    public IReadOnlyCollection<GitHook> GetPushHooks();

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Merge"/> category.
    /// </summary>
    /// <returns><c>pre-merge-commit</c>, <c>post-merge</c>.</returns>
    public IReadOnlyCollection<GitHook> GetMergeHooks();

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Rebase"/> category.
    /// </summary>
    /// <returns><c>pre-rebase</c>, <c>post-rewrite</c>.</returns>
    public IReadOnlyCollection<GitHook> GetRebaseHooks();

    /// <summary>
    /// Returns all hooks in the <see cref="GitHookCategory.Checkout"/> category.
    /// </summary>
    /// <returns><c>post-checkout</c>, <c>post-index-change</c>.</returns>
    public IReadOnlyCollection<GitHook> GetCheckoutHooks();
}
