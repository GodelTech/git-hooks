namespace GitHooks.Infrastructure.GitHooks;

/// <summary>
/// Provides a read-only catalog of well-known Git hooks grouped by lifecycle category.
/// </summary>
/// <remarks>
/// Inject this interface wherever Git hook metadata is needed. Use
/// <see cref="GetHooks(GitHookCategory)"/> as the primary programmatic entry point, or use
/// the typed convenience extension methods on <see cref="GitHookCatalogExtensions"/> when the
/// category is known at compile time.
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
    /// Resolves hook names provided by a caller into known hooks and invalid names.
    /// </summary>
    /// <param name="hooks">Raw hook names from user input or configuration.</param>
    /// <returns>
    /// A <see cref="GitHookResolutionResult"/> that separates normalized supported hooks
    /// as <see cref="GitHook"/> models from normalized unsupported hook names.
    /// </returns>
    public GitHookResolutionResult ResolveHooks(IReadOnlyCollection<string> hooks);
}
