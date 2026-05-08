using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Hooks;
using GitHooks.Infrastructure.Scaffold;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="ICreateHookHandler"/>.
/// </summary>
public sealed class CreateHookHandler(
    IGitCommandLine gitCommandLine,
    IGitHookCatalog gitHookCatalog,
    IGitHookScaffolder gitHookScaffolder,
    IAnsiConsole console) : ICreateHookHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IGitHookCatalog _gitHookCatalog = gitHookCatalog;
    private readonly IGitHookScaffolder _gitHookScaffolder = gitHookScaffolder;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(GitConfigScope scope, string hooksPath, IReadOnlyCollection<string> hooks, bool force, CancellationToken cancellationToken = default)
    {
        if (!await _gitCommandLine.IsAvailableAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH. Install Git and retry.");
            return 1;
        }

        if (!await _gitCommandLine.IsInsideGitRepositoryAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Not inside a git repository. The [blue]create[/] command requires running from within a git repository.");
            return 1;
        }

        var repositoryRootPathResult = await _gitCommandLine.GetRepositoryRootPathAsync(cancellationToken);

        if (!repositoryRootPathResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to resolve repository root path: {repositoryRootPathResult.Error.Trim()}");
            return 1;
        }

        var repositoryRootPath = repositoryRootPathResult.Output.Trim();

        if (string.IsNullOrWhiteSpace(repositoryRootPath))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git returned an empty repository root path.");
            return 1;
        }

        var hookValidationResult = _gitHookCatalog.ResolveHooks(hooks);

        if (!hookValidationResult.IsValid)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Unsupported hook name(s): [blue]{string.Join(", ", hookValidationResult.InvalidHooks)}[/].");
            _console.MarkupLine("[yellow][[INFO]][/] Check the full list in [blue]docs/git-hooks-reference.md[/].");
            return 1;
        }

        var creationResult = await _gitHookScaffolder.CreateScaffoldAsync(
            repositoryRootPath,
            hooksPath,
            hookValidationResult.SupportedHooks,
            force,
            cancellationToken
        );

        if (!creationResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {creationResult.Error}");

            if (!force)
            {
                _console.MarkupLine("[yellow][[INFO]][/] Use [blue]--force[/] to overwrite existing files.");
            }

            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[SUCCESS]][/] Created {creationResult.CreatedHooks.Count} hook file(s) in: [blue]{hooksPath}[/] ([grey]{creationResult.ResolvedHooksPath}[/])");
        _console.MarkupLineInterpolated($"[green][[OK]][/] Hooks: [blue]{string.Join(", ", creationResult.CreatedHooks.Select(h => h.Name))}[/]");

        return 0;
    }
}
