using GitHooks.Infrastructure;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="IUninstallHandler"/>.
/// </summary>
public sealed class UninstallHandler(
    IGitCommandLine gitCommandLine,
    IAnsiConsole console) : IUninstallHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
    {
        if (!await _gitCommandLine.IsAvailableAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH. Install Git and retry.");
            return 1;
        }

        if (scope == GitConfigScope.Local && !await _gitCommandLine.IsInsideGitRepositoryAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Not inside a git repository. The [blue]local[/] scope requires running from within a git repository.");
            return 1;
        }

        var existing = await _gitCommandLine.GetCoreHooksPathAsync(scope, cancellationToken);

        if (!existing.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[green][[OK]][/] {scope.ToGitString()} core.hooksPath is not configured. Nothing to uninstall.");
            return 0;
        }

        var existingValue = existing.Output.Trim();

        var unsetResult = await _gitCommandLine.UnsetCoreHooksPathAsync(scope, cancellationToken);

        if (!unsetResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to unset {scope.ToGitString()} core.hooksPath: {unsetResult.Error.Trim()}");
            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[SUCCESS]][/] {scope.ToGitString()} core.hooksPath ([blue]{existingValue}[/]) has been removed.");
        return 0;
    }
}

