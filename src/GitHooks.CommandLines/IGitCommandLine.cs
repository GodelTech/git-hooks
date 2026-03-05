namespace GitHooks.CommandLines;

public interface IGitCommandLine
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<string> GetGlobalHooksPathAsync(CancellationToken cancellationToken = default);

    Task SetGlobalHooksPathAsync(string hooksPath, CancellationToken cancellationToken = default);
}
