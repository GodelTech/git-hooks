using GitHooks.Infrastructure;
using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.GitHooks;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="ICreateHookHandler"/>.
/// </summary>
public sealed class CreateHookHandler(
    IGitCommandLine gitCommandLine,
    IGitHookCatalog gitHookCatalog,
    IHookFileManager hookFileManager,
    IAnsiConsole console) : ICreateHookHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IGitHookCatalog _gitHookCatalog = gitHookCatalog;
    private readonly IHookFileManager _hookFileManager = hookFileManager;
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

        var normalizedHooks = NormalizeHooks(hooks);
        var supportedHookNames = _gitHookCatalog.GetAllHooks().Select(h => h.Name).ToHashSet(StringComparer.Ordinal);
        var invalidHooks = normalizedHooks.Where(hook => !supportedHookNames.Contains(hook)).ToArray();

        if (invalidHooks.Length > 0)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Unsupported hook name(s): [blue]{string.Join(", ", invalidHooks)}[/].");
            _console.MarkupLine("[yellow][[INFO]][/] Check the full list in [blue]docs/git-hooks-reference.md[/].");
            return 1;
        }

        var creationResult = await _hookFileManager.CreateHookFilesAsync(repositoryRootPath, hooksPath, normalizedHooks, force, cancellationToken);

        if (!creationResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {creationResult.Error}");
            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[SUCCESS]][/] Created {creationResult.CreatedHooks.Count} hook file(s) in: [blue]{hooksPath}[/] ([grey]{creationResult.ResolvedHooksPath}[/])");
        _console.MarkupLineInterpolated($"[green][[OK]][/] Hooks: [blue]{string.Join(", ", creationResult.CreatedHooks)}[/]");

        return 0;
    }

    private static string[] NormalizeHooks(IReadOnlyCollection<string> hooks)
    {
        return [.. hooks
            .Select(hook => hook.Trim().ToLowerInvariant())
            .Where(hook => !string.IsNullOrWhiteSpace(hook))
            .Distinct(StringComparer.Ordinal)];
    }
}
