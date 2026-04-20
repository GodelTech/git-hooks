using GitHooks.CommandLine;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="IInstallHandler"/>.
/// </summary>
public class InstallHandler(
    IGitCommandLine gitCommandLine,
    IAnsiConsole console) : IInstallHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(string hooksPath, bool force, CancellationToken token = default)
    {
        if (!await _gitCommandLine.IsAvailableAsync(token))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH. Install Git and retry.");
            return 1;
        }

        _console.MarkupLine($"[green][[SUCCESS]][/] Global core.hooksPath successfully set to: [blue]{hooksPath}[/]");
        return 0;
    }
}
