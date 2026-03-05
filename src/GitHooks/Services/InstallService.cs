using GitHooks.CommandLines;
using GitHooks.Commands;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitHooks.Services;

public class InstallService : IInstallService
{
    private readonly IGitCommandLine _gitCommandLine;
    private readonly IAnsiConsole _console;

    public InstallService(IGitCommandLine gitCommandLine, IAnsiConsole console)
    {
        _gitCommandLine = gitCommandLine;
        _console = console;
    }

    public async Task<ServiceResult> RunAsync(string hooksPath, bool force, CancellationToken cancellationToken = default)
    {
        // Verify Git is available
        if (!await _gitCommandLine.IsAvailableAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH. Install Git and retry.");
            return ServiceResult.Failure("Git not found in PATH");
        }

        // Check current global core.hooksPath setting
        var currentGlobalHooksPath = await _gitCommandLine.GetGlobalHooksPathAsync(cancellationToken);

        if (string.Equals(currentGlobalHooksPath, hooksPath, StringComparison.OrdinalIgnoreCase))
        {
            _console.MarkupLine($"[green][[OK]][/] Global core.hooksPath is already set to the specified path: [blue]{hooksPath}[/]");
            return ServiceResult.Success();
        }

        if (!force)
        {
            _console.MarkupLine($"[yellow][[WARNING]][/] Global core.hooksPath is currently set to: [blue]{currentGlobalHooksPath}[/]");
            _console.MarkupLine($"[yellow][[WARNING]][/] You are about to change it to: [blue]{hooksPath}[/]");
            bool confirmed = _console.Confirm($"Overwrite with [blue]{hooksPath}[/]?");

            if (!confirmed)
            {
                _console.MarkupLine("[yellow][[CANCELLED]][/] Operation cancelled by user.");
                return ServiceResult.Success();
            }
        }

        // Set the global core.hooksPath setting
        var success = false;
        await _console.Status()
            .Start(
                $"Setting global core.hooksPath to: [blue]{hooksPath}[/]...",
                async context =>
                {
                    try
                    {
                        await _gitCommandLine.SetGlobalHooksPathAsync(hooksPath, cancellationToken);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                    }
                }
            );

        if (!success)
        {
            _console.MarkupLine($"[red][[ERROR]][/] Failed to set global core.hooksPath.");
            return ServiceResult.Failure("Failed to set global core.hooksPath");
        }

        // Verify the global core.hooksPath setting was applied
        var verifiedGlobalHooksPath = await _gitCommandLine.GetGlobalHooksPathAsync(cancellationToken);
        if (!string.Equals(verifiedGlobalHooksPath, hooksPath, StringComparison.OrdinalIgnoreCase))
        {
            _console.MarkupLine($"[red][[ERROR]][/] Verification failed. Expected: [blue]{hooksPath}[/], Actual: [blue]{verifiedGlobalHooksPath}[/]");
            return ServiceResult.Failure("Verification of global core.hooksPath failed");
        }

        _console.MarkupLine($"[green][[SUCCESS]][/] Global core.hooksPath successfully set to: [blue]{hooksPath}[/]");
        return ServiceResult.Success();
    }
}
