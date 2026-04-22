using GitHooks.Infrastructure.Cli;

namespace GitHooks.Infrastructure.Git;

/// <summary>
/// Default implementation of <see cref="IGitCommandLine"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GitCommandLine"/> class.
/// </remarks>
/// <param name="commandLine">The command-line used to execute git commands.</param>
public sealed class GitCommandLine(ICommandLine commandLine) : IGitCommandLine
{
    private readonly ICommandLine _commandLine = commandLine;

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var result = await _commandLine.RunAsync("git", ["--version"], cancellationToken);

        return result.IsSuccess;
    }

    /// <inheritdoc/>
    public async Task<bool> IsInsideGitRepositoryAsync(CancellationToken cancellationToken = default)
    {
        var result = await _commandLine.RunAsync("git", ["rev-parse", "--is-inside-work-tree"], cancellationToken);

        return result.IsSuccess;
    }

    /// <inheritdoc/>
    public Task<CommandLineResult> GetRepositoryRootPathAsync(CancellationToken cancellationToken = default)
    {
        return _commandLine.RunAsync("git", ["rev-parse", "--show-toplevel"], cancellationToken);
    }

    /// <inheritdoc/>
    public Task<CommandLineResult> GetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
    {
        return _commandLine.RunAsync("git", ["config", $"--{scope.ToGitString()}", "--get", "core.hooksPath"], cancellationToken);
    }

    /// <inheritdoc/>
    public Task<CommandLineResult> SetCoreHooksPathAsync(GitConfigScope scope, string value, CancellationToken cancellationToken = default)
    {
        return _commandLine.RunAsync("git", ["config", $"--{scope.ToGitString()}", "core.hooksPath", value], cancellationToken);
    }

    /// <inheritdoc/>
    public Task<CommandLineResult> UnsetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
    {
        return _commandLine.RunAsync("git", ["config", $"--{scope.ToGitString()}", "--unset", "core.hooksPath"], cancellationToken);
    }
}
