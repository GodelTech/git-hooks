namespace GitHooks.Infrastructure.Hooks;

/// <summary>
/// Default implementation of <see cref="IGitHookCatalog"/> containing metadata for all
/// well-known Git hooks in the five supported lifecycle categories.
/// </summary>
/// <remarks>
/// Hook data is stored in a single static list, built once per process.
/// Use <see cref="Default"/> for non-DI contexts; register via DI as
/// <c>services.AddSingleton&lt;IGitHookCatalog, GitHookCatalog&gt;()</c> in hosted applications.
/// </remarks>
public sealed class GitHookCatalog : IGitHookCatalog
{
    // Full catalog — all 11 hooks across the five supported categories.
    // CanAbort = true means a non-zero exit code cancels the Git operation.
    // Static so the list and lookup dictionary are built once per process, not per instance.
    private static readonly IReadOnlyCollection<GitHook> s_all =
    [

        // Commit hooks
        new("pre-commit",         GitHookCategory.Commit,   "Runs before the commit message editor opens. Aborts the commit on non-zero exit.",                 CanAbort: true),
        new("prepare-commit-msg", GitHookCategory.Commit,   "Runs before the commit editor with the default message. Used to modify the message template.",     CanAbort: true),
        new("commit-msg",         GitHookCategory.Commit,   "Receives the commit message file path. Used to validate or normalise the commit message.",         CanAbort: true),
        new("post-commit",        GitHookCategory.Commit,   "Runs after the commit completes. Typically used for notifications or side-effects; cannot abort.", CanAbort: false),

        // Push hooks
        new("pre-push",           GitHookCategory.Push,     "Runs during git push before objects are transferred. Can abort the push on non-zero exit.",        CanAbort: true),

        // Merge hooks
        new("pre-merge-commit",   GitHookCategory.Merge,    "Runs before a merge commit is created. Aborts the merge on non-zero exit.",                        CanAbort: true),
        new("post-merge",         GitHookCategory.Merge,    "Runs after a successful merge. Not called for fast-forward merges; cannot abort.",                 CanAbort: false),

        // Rebase hooks
        new("pre-rebase",         GitHookCategory.Rebase,   "Runs before a rebase starts. Aborts the rebase on non-zero exit.",                                 CanAbort: true),
        new("post-rewrite",       GitHookCategory.Rebase,   "Runs after commands that rewrite commits (amend, rebase). Informational; cannot abort.",           CanAbort: false),

        // Checkout hooks
        new("post-checkout",      GitHookCategory.Checkout, "Runs after git checkout or git switch. Not called for file-level checkouts; cannot abort.",        CanAbort: false),
        new("post-index-change",  GitHookCategory.Checkout, "Runs after the staging area (index) is written to disk. Informational; cannot abort.",             CanAbort: false),
    ];

    private static readonly Dictionary<string, GitHook> s_hooksByName = s_all.ToDictionary(hook => hook.Name, StringComparer.Ordinal);

    private static readonly Dictionary<GitHookCategory, IReadOnlyCollection<GitHook>> s_hooksByCategory =
        s_all
            .GroupBy(hook => hook.Category)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<GitHook>)[.. g]);

    /// <summary>
    /// Gets a shared singleton instance for use outside of dependency injection containers.
    /// </summary>
    public static IGitHookCatalog Default { get; } = new GitHookCatalog();

    /// <inheritdoc/>
    public IReadOnlyCollection<GitHook> GetAllHooks()
    {
        return s_all;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<GitHook> GetHooks(GitHookCategory category)
    {
        return s_hooksByCategory.TryGetValue(category, out var hooks)
            ? hooks
            : [];
    }

    /// <inheritdoc/>
    public GitHookResolutionResult ResolveHooks(IReadOnlyCollection<string> hooks)
    {
        var normalizedHooks = hooks
            .Select(hook => hook.Trim().ToLowerInvariant())
            .Where(hook => !string.IsNullOrWhiteSpace(hook))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var resolved = new List<GitHook>();
        var invalid = new List<string>();

        foreach (var hook in normalizedHooks)
        {
            // Single TryGetValue per hook — avoids the double-lookup of ContainsKey + indexer.
            if (s_hooksByName.TryGetValue(hook, out var gitHook))
            {
                resolved.Add(gitHook);
            }
            else
            {
                invalid.Add(hook);
            }
        }

        return new GitHookResolutionResult(resolved, invalid);
    }
}
