using GitHooks.Infrastructure.Git;
using GitHooks.Pipeline.Domain;
using GitHooks.Pipeline.Execution;
using GitHooks.Pipeline.Transformation;

using IPipelineParser = GitHooks.Workflow.Application.Parsing.IPipelineParser;
using IPipelineParserOld = GitHooks.Pipeline.Parsing.IPipelineParser;

using Spectre.Console;

namespace GitHooks.Handlers;

public sealed class RunHandler(
    IGitCommandLine gitCommandLine,
    IPipelineParser pipelineParser,
    IPipelineParserOld pipelineParserOld,
    ITemplateExpander templateExpander,
    IPipelineRunner pipelineRunner,
    IAnsiConsole console) : IRunHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IPipelineParser _pipelineParser = pipelineParser;
    private readonly IPipelineParserOld _pipelineParserOld = pipelineParserOld;
    private readonly ITemplateExpander _templateExpander = templateExpander;
    private readonly IPipelineRunner _pipelineRunner = pipelineRunner;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(string filePath, CancellationToken cancellationToken = default)
    {
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

#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var a = _pipelineParser.Parse(yamlContent, absoluteFilePath);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

        try
        {
            // Stage 1: Parse YAML to typed AST.
            var pipeline = _pipelineParserOld.Parse(yamlContent, absoluteFilePath);

            // Stage 2: Expand template steps recursively.
            var expanded = await _templateExpander.ExpandAsync(pipeline, absoluteFilePath, cancellationToken);

            // Stage 3: Execute expanded script steps.
            var result = await _pipelineRunner.RunAsync(expanded, cancellationToken);

            if (!result.IsSuccess)
            {
                var failedStep = result.FailedStepId.HasValue ? $" at step {result.FailedStepId.Value}" : string.Empty;
                _console.MarkupLineInterpolated($"[red][[ERROR]][/] Pipeline execution failed{failedStep}: {Markup.Escape(result.ErrorMessage ?? "Unknown error.")}");
                return 1;
            }

            _console.MarkupLine("[green][[SUCCESS]][/] Pipeline completed successfully.");
            return 0;
        }
        catch (PipelineException ex)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}
