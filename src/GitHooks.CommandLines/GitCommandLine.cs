namespace GitHooks.CommandLines;

public class GitCommandLine : CommandLineBase, IGitCommandLine
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunAsync("git", "--version", cancellationToken);

        return result.IsSuccess;
    }

    public async Task<string> GetGlobalHooksPathAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunAsync("git", "config --global --get core.hooksPath", cancellationToken);

        return result.Output.Trim();
    }

    public async Task SetGlobalHooksPathAsync(string hooksPath, CancellationToken cancellationToken = default)
    {
        var result = await RunAsync("git", $"config --global core.hooksPath {hooksPath}", cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to set global hooks path: {result.Error}");
        }
    }
}
