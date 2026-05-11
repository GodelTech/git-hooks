using GitHooks.Infrastructure.Git;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="IInstallHandler"/>.
/// </summary>
public sealed class InstallHandler(
    IGitCommandLine gitCommandLine,
    IAnsiConsole console) : IInstallHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(GitConfigScope scope, string hooksPath, bool force, CancellationToken cancellationToken = default)
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

        if (existing.IsSuccess)
        {
            var existingValue = existing.Output.Trim();

            if (string.Equals(existingValue, hooksPath, StringComparison.Ordinal))
            {
                _console.MarkupLineInterpolated($"[green][[OK]][/] {scope.ToGitString()} core.hooksPath is already set to: [blue]{existingValue}[/]");
                return 0;
            }

            if (!force)
            {
                _console.MarkupLineInterpolated($"[red][[ERROR]][/] {scope.ToGitString()} core.hooksPath is already set to: [blue]{existingValue}[/]. Use --force to overwrite.");
                return 1;
            }
        }

        var setResult = await _gitCommandLine.SetCoreHooksPathAsync(scope, hooksPath, cancellationToken);

        if (!setResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to set {scope.ToGitString()} core.hooksPath: {setResult.Error.Trim()}");
            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[SUCCESS]][/] {scope.ToGitString()} core.hooksPath successfully set to: [blue]{hooksPath}[/]");
        return 0;
    }
}
