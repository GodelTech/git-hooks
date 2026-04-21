using GitHooks.CommandLine;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="ICreateHookHandler"/>.
/// </summary>
public sealed class CreateHookHandler(
    IGitCommandLine gitCommandLine,
    IHookFileManager hookFileManager,
    IAnsiConsole console) : ICreateHookHandler
{
    private static readonly HashSet<string> SupportedHooks =
    [
        "applypatch-msg",
        "pre-applypatch",
        "post-applypatch",
        "pre-commit",
        "pre-merge-commit",
        "prepare-commit-msg",
        "commit-msg",
        "post-commit",
        "pre-rebase",
        "post-rewrite",
        "post-checkout",
        "post-merge",
        "pre-push",
        "pre-receive",
        "update",
        "proc-receive",
        "post-receive",
        "post-update",
        "reference-transaction",
        "push-to-checkout",
        "pre-auto-gc",
        "post-index-change",
        "sendemail-validate",
        "fsmonitor-watchman",
        "p4-changelist",
        "p4-prepare-changelist",
        "p4-post-changelist",
        "p4-pre-submit"
    ];

    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
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

        if (scope == GitConfigScope.Local && !await _gitCommandLine.IsInsideGitRepositoryAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Not inside a git repository. The [blue]local[/] scope requires running from within a git repository.");
            return 1;
        }

        var normalizedHooks = NormalizeHooks(hooks);
        var invalidHooks = normalizedHooks.Where(hook => !SupportedHooks.Contains(hook)).ToArray();

        if (invalidHooks.Length > 0)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Unsupported hook name(s): [blue]{string.Join(", ", invalidHooks)}[/].");
            _console.MarkupLine("[yellow][[INFO]][/] Check the full list in [blue]docs/git-hooks-reference.md[/].");
            return 1;
        }

        var creationResult = await _hookFileManager.CreateHookFilesAsync(hooksPath, normalizedHooks, force, cancellationToken);

        if (!creationResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {creationResult.Error}");
            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[SUCCESS]][/] Created {creationResult.CreatedHooks.Count} hook file(s) in: [blue]{hooksPath}[/]");
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
