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
    public async Task<int> HandleAsync(string scope, string hooksPath, bool force, CancellationToken cancellationToken = default)
    {
        if (!await _gitCommandLine.IsAvailableAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH. Install Git and retry.");
            return 1;
        }

        var existing = await _gitCommandLine.GetCoreHooksPathAsync(scope, cancellationToken);

        if (existing.IsSuccess)
        {
            var existingValue = existing.Output.Trim();

            if (string.Equals(existingValue, hooksPath, StringComparison.Ordinal))
            {
                _console.MarkupLine($"[green][[OK]][/] core.hooksPath is already set to: [blue]{existingValue}[/]");
                return 0;
            }

            if (!force)
            {
                _console.MarkupLine($"[red][[ERROR]][/] core.hooksPath is already set to: [blue]{existingValue}[/]. Use --force to overwrite.");
                return 1;
            }
        }

        var setResult = await _gitCommandLine.SetCoreHooksPathAsync(scope, hooksPath, cancellationToken);

        if (!setResult.IsSuccess)
        {
            _console.MarkupLine($"[red][[ERROR]][/] Failed to set core.hooksPath: {setResult.Error.Trim()}");
            return 1;
        }

        _console.MarkupLine($"[green][[SUCCESS]][/] {scope} core.hooksPath successfully set to: [blue]{hooksPath}[/]");
        return 0;
    }
}
