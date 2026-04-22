using GitHooks.Infrastructure.Git;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="IRunHandler"/>.
/// </summary>
public sealed class RunHandler(
    IGitCommandLine gitCommandLine,
    IAnsiConsole console) : IRunHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var startupWorkingDirectory = Environment.CurrentDirectory;

        if (!await _gitCommandLine.IsAvailableAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH. Install Git and retry.");
            return 1;
        }

        if (!await _gitCommandLine.IsInsideGitRepositoryAsync(cancellationToken))
        {
            _console.MarkupLine("[red][[ERROR]][/] Not inside a git repository. The [blue]run[/] command requires running from within a git repository.");
            return 1;
        }

        var repositoryRootResult = await _gitCommandLine.GetTopLevelAsync(cancellationToken);

        if (!repositoryRootResult.IsSuccess)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to resolve repository root path: {repositoryRootResult.Error.Trim()}");
            return 1;
        }

        var repositoryRootPath = repositoryRootResult.Output.Trim();

        if (string.IsNullOrWhiteSpace(repositoryRootPath))
        {
            _console.MarkupLine("[red][[ERROR]][/] Failed to resolve repository root path.");
            return 1;
        }

        var absoluteFilePath = Path.GetFullPath(filePath);

        if (!File.Exists(absoluteFilePath))
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] YAML file not found: [blue]{Markup.Escape(absoluteFilePath)}[/]");
            return 1;
        }

        var relativeFilePath = Path.GetRelativePath(repositoryRootPath, absoluteFilePath);
        string yamlContent;

        try
        {
            yamlContent = await File.ReadAllTextAsync(absoluteFilePath, cancellationToken);
        }
        catch (IOException ex)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to read YAML file: {Markup.Escape(ex.Message)}");
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to read YAML file: {Markup.Escape(ex.Message)}");
            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[OK]][/] Started from: [blue]{Markup.Escape(startupWorkingDirectory)}[/]");
        _console.MarkupLineInterpolated($"[green][[OK]][/] Repository root: [blue]{Markup.Escape(repositoryRootPath)}[/]");
        _console.MarkupLineInterpolated($"[green][[OK]][/] YAML relative path: [blue]{Markup.Escape(relativeFilePath)}[/]");
        _console.MarkupLine("[green][[OK]][/] YAML content:");
        _console.WriteLine(yamlContent);

        return 0;
    }
}
