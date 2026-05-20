using GitHooks.Infrastructure.Git;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Application.Resolution;
using GitHooks.Workflow.Domain.Model;

using Spectre.Console;

namespace GitHooks.Handlers;

public sealed class RunHandler(
    IGitCommandLine gitCommandLine,
    IPipelineResolver pipelineResolver,
    IAnsiConsole console)
    : IRunHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly IPipelineResolver _pipelineResolver = pipelineResolver;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(
        string filePath,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default)
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

        try
        {
            var pipelineSource = PipelineSource.LocalFile(absoluteFilePath);
            var resolvedPipeline = await _pipelineResolver.ResolveAsync(pipelineSource, parameterOverrides, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to read YAML file: {Markup.Escape(ex.Message)}");
            return 1;
        }
        catch (PipelineParsingException ex)
        {
            var location = ex.Span.Start.Line > 0
                ? $"{Markup.Escape(ex.Span.Source.Name)}:{ex.Span.Start.Line}:{ex.Span.Start.Column}: "
                : string.Empty;

            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {location}{Markup.Escape(ex.Message)}");
            return 1;
        }
        catch (PipelineParameterBindingException ex)
        {
            var location = ex.Span.Start.Line > 0
                ? $"{Markup.Escape(ex.Span.Source.Name)}:{ex.Span.Start.Line}:{ex.Span.Start.Column}: "
                : string.Empty;

            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {location}{Markup.Escape(ex.Message)}");
            return 1;
        }
        catch (PipelineTemplateExpansionException ex)
        {
            var location = ex.Span.Start.Line > 0
                ? $"{Markup.Escape(ex.Span.Source.Name)}:{ex.Span.Start.Line}:{ex.Span.Start.Column}: "
                : string.Empty;

            var includeChain = ex.IncludeChain.Count > 0
                ? $" Include chain: {string.Join(" -> ", ex.IncludeChain.Select(source => Markup.Escape(source.Identifier)))}."
                : string.Empty;

            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {location}{Markup.Escape(ex.Message)}{includeChain}");
            return 1;
        }

        // try
        // {
        //    // Stage 1: Parse YAML to typed AST.
        //    var pipeline = _pipelineParserOld.Parse(yamlContent, absoluteFilePath);

        // // Stage 2: Expand template steps recursively.
        //    var expanded = await _templateExpander.ExpandAsync(pipeline, absoluteFilePath, cancellationToken);

        // // Stage 3: Execute expanded script steps.
        //    var result = await _pipelineRunner.RunAsync(expanded, cancellationToken);

        // if (!result.IsSuccess)
        //    {
        //        var failedStep = result.FailedStepId.HasValue ? $" at step {result.FailedStepId.Value}" : string.Empty;
        //        _console.MarkupLineInterpolated($"[red][[ERROR]][/] Pipeline execution failed{failedStep}: {Markup.Escape(result.ErrorMessage ?? "Unknown error.")}");
        //        return 1;
        //    }

        // _console.MarkupLine("[green][[SUCCESS]][/] Pipeline completed successfully.");
        //    return 0;
        // }
        // catch (PipelineException ex)
        // {
        //    _console.MarkupLineInterpolated($"[red][[ERROR]][/] {Markup.Escape(ex.Message)}");
        //    return 1;
        // }
        return 0;
    }
}
