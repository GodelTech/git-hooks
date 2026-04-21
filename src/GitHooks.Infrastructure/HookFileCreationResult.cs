namespace GitHooks.Infrastructure;

/// <summary>
/// Represents the result of creating hook files.
/// </summary>
public sealed class HookFileCreationResult
{
    private HookFileCreationResult(bool isSuccess, IReadOnlyCollection<string> createdHooks, string error)
    {
        IsSuccess = isSuccess;
        CreatedHooks = createdHooks;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether hook file creation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the hook names that were created or overwritten.
    /// </summary>
    public IReadOnlyCollection<string> CreatedHooks { get; }

    /// <summary>
    /// Gets the failure details when the operation does not succeed.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Creates a successful result instance.
    /// </summary>
    /// <param name="createdHooks">The hook names that were created.</param>
    /// <returns>A successful <see cref="HookFileCreationResult"/>.</returns>
    public static HookFileCreationResult Success(IReadOnlyCollection<string> createdHooks)
    {
        return new HookFileCreationResult(true, createdHooks, string.Empty);
    }

    /// <summary>
    /// Creates a failed result instance.
    /// </summary>
    /// <param name="error">The failure details.</param>
    /// <returns>A failed <see cref="HookFileCreationResult"/>.</returns>
    public static HookFileCreationResult Failure(string error)
    {
        return new HookFileCreationResult(false, [], error);
    }
}

