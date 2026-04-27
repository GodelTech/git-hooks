using GitHooks.Infrastructure.Git;
using GitHooks.Pipeline.Contracts;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// Default implementation of <see cref="IRunHandler"/>.
/// </summary>
public sealed class RunHandler(
    IGitCommandLine gitCommandLine,
    IYamlPipelineParser yamlPipelineParser,
    IPipelineCompiler pipelineCompiler,
    IStepRunner stepRunner,
    IAnsiConsole console) : IRunHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IYamlPipelineParser _yamlPipelineParser = yamlPipelineParser;
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
    private readonly IStepRunner _stepRunner = stepRunner;
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
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to read YAML file: {Markup.Escape(ex.Message)}");
            return 1;
        }

        var parseResult = _yamlPipelineParser.Parse(yamlContent);

        if (!parseResult.IsSuccess || parseResult.Pipeline is null)
        {
            _console.MarkupLine("[red][[ERROR]][/] YAML validation failed.");
            foreach (var error in parseResult.Errors)
            {
                _console.MarkupLineInterpolated($"{Markup.Escape(absoluteFilePath)}({error.Line},{error.Column}): [red]error[/]: {Markup.Escape(error.Message)}");
            }

            return 1;
        }

        var compileRequest = new CompileRequest(
            Pipeline: parseResult.Pipeline,
            QueueParameters: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            SourcePath: absoluteFilePath);

        var compileResult = _pipelineCompiler.Compile(compileRequest);

        if (!compileResult.IsSuccess || compileResult.Plan is null)
        {
            _console.MarkupLine("[red][[ERROR]][/] Pipeline compilation failed.");
            foreach (var error in compileResult.Errors)
            {
                _console.MarkupLineInterpolated($"{Markup.Escape(absoluteFilePath)}({error.Line},{error.Column}): [red]error[/]: {Markup.Escape(error.Message)}");
            }

            return 1;
        }

        _console.MarkupLineInterpolated($"[green][[OK]][/] Started from: [blue]{Markup.Escape(startupWorkingDirectory)}[/]");
        _console.MarkupLineInterpolated($"[green][[OK]][/] Repository root: [blue]{Markup.Escape(repositoryRootPath)}[/]");
        _console.MarkupLineInterpolated($"[green][[OK]][/] YAML relative path: [blue]{Markup.Escape(relativeFilePath)}[/]");
        _console.MarkupLineInterpolated($"[green][[OK]][/] Parsed [blue]{parseResult.Pipeline.Parameters.Count}[/] parameter(s) and [blue]{parseResult.Pipeline.Steps.Count}[/] step(s).");
        _console.MarkupLineInterpolated($"[green][[OK]][/] Compiled [blue]{compileResult.Plan.Parameters.Count}[/] parameter value(s) and [blue]{compileResult.Plan.Steps.Count}[/] executable step(s).");
        _console.MarkupLine("[green][[OK]][/] YAML validation passed.");
        _console.MarkupLine("[green][[OK]][/] Running steps.");

        return await _stepRunner.RunAsync(compileResult.Plan, cancellationToken);
    }
}
