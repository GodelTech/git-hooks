using GitHooks.Infrastructure.Hooks;

namespace GitHooks.Infrastructure.Scaffold;

/// <summary>
/// Represents the result of creating hook scaffold files.
/// </summary>
public sealed class GitHookScaffoldResult
{
    private GitHookScaffoldResult(bool isSuccess, IReadOnlyCollection<GitHook> createdHooks, string error, string resolvedHooksPath)
    {
        IsSuccess = isSuccess;
        CreatedHooks = createdHooks;
        Error = error;
        ResolvedHooksPath = resolvedHooksPath;
    }

    /// <summary>
    /// Gets a value indicating whether hook scaffold creation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the hooks that were created or overwritten.
    /// </summary>
    public IReadOnlyCollection<GitHook> CreatedHooks { get; }

    /// <summary>
    /// Gets the failure details when the operation does not succeed.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets the absolute resolved path where hook scaffold files were created.
    /// Only meaningful when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public string ResolvedHooksPath { get; }

    /// <summary>
    /// Creates a successful result instance.
    /// </summary>
    /// <param name="createdHooks">The hooks that were created.</param>
    /// <param name="resolvedHooksPath">The absolute resolved path where hook scaffold files were created.</param>
    /// <returns>A successful <see cref="GitHookScaffoldResult"/>.</returns>
    public static GitHookScaffoldResult Success(IReadOnlyCollection<GitHook> createdHooks, string resolvedHooksPath)
    {
        return new GitHookScaffoldResult(true, createdHooks, string.Empty, resolvedHooksPath);
    }

    /// <summary>
    /// Creates a failed result instance.
    /// </summary>
    /// <param name="error">The failure details.</param>
    /// <returns>A failed <see cref="GitHookScaffoldResult"/>.</returns>
    public static GitHookScaffoldResult Failure(string error)
    {
        return new GitHookScaffoldResult(false, [], error, string.Empty);
    }
}

