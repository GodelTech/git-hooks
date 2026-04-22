namespace GitHooks.Infrastructure.Hooks;

/// <summary>
/// Represents a single Git hook with its metadata.
/// </summary>
/// <param name="Name">
/// The Git hook name as used in the <c>.githooks/</c> directory (e.g. <c>pre-commit</c>).
/// </param>
/// <param name="Category">
/// The lifecycle category this hook belongs to.
/// </param>
/// <param name="Description">
/// A human-readable description of when the hook fires and what it is typically used for.
/// </param>
/// <param name="CanAbort">
/// <see langword="true"/> when a non-zero exit code from this hook will cancel the
/// in-progress Git operation; <see langword="false"/> when Git ignores the exit code.
/// </param>
/// <example>
/// <code>
/// GitHook hook = new("pre-commit", GitHookCategory.Commit, "Runs before the commit message editor opens.", true);
/// Console.WriteLine(hook.Name);     // pre-commit
/// Console.WriteLine(hook.CanAbort); // True
/// </code>
/// </example>
public sealed record GitHook(
    string Name,
    GitHookCategory Category,
    string Description,
    bool CanAbort
);
