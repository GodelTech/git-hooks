namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="IInstallHandler"/>.
/// </summary>
public class InstallHandler : IInstallHandler
{
    /// <inheritdoc/>
    public Task<int> HandleAsync(string hooksPath, bool force, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
