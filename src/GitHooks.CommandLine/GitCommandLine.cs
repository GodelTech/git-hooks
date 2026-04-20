namespace GitHooks.CommandLine;

/// <summary>
/// Default implementation of <see cref="IGitCommandLine"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GitCommandLine"/> class.
/// </remarks>
/// <param name="commandLineRunner">The command-line runner used to execute git commands.</param>
public class GitCommandLine(ICommandLineRunner commandLineRunner) : IGitCommandLine
{
    private readonly ICommandLineRunner _commandLineRunner = commandLineRunner;

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var result = await _commandLineRunner.RunAsync("git", "--version", cancellationToken);

        return result.IsSuccess;
    }

    /// <inheritdoc/>
    public Task<CommandLineResult> GetCoreHooksPathAsync(string scope, CancellationToken cancellationToken = default)
    {
        return _commandLineRunner.RunAsync("git", $"config --{scope} --get core.hooksPath", cancellationToken);
    }

    /// <inheritdoc/>
    public Task<CommandLineResult> SetCoreHooksPathAsync(string scope, string value, CancellationToken cancellationToken = default)
    {
        return _commandLineRunner.RunAsync("git", $"config --{scope} --set core.hooksPath \"{value}\"", cancellationToken);
    }
}
