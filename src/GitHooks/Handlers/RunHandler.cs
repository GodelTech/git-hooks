using GitHooks.Compilation;
using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Git;

using Spectre.Console;

namespace GitHooks.Handlers;

public sealed class RunHandler(
    IGitCommandLine gitCommandLine,
    PipelineCompiler compiler,
    IAnsiConsole console)
    : IRunHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly PipelineCompiler _compiler = compiler;
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
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] YAML file not found: [blue]{absoluteFilePath}[/]");
            return 1;
        }

        string yamlText;
        SourceDocument document;

        try
        {
            yamlText = await File.ReadAllTextAsync(absoluteFilePath, cancellationToken);
            document = new SourceDocument(absoluteFilePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Failed to read YAML file: {exception.Message}");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(yamlText))
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] YAML file is empty: [blue]{absoluteFilePath}[/]");
            return 1;
        }

        var context = new CompilationContext();

        CompilationResult result;

        try
        {
            result = _compiler.Compile(
                yamlText,
                document,
                context,
                parameterOverrides);
        }
        catch (ArgumentException exception)
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] Invalid compilation input: {exception.Message}");
            return 1;
        }

        foreach (var diagnostic in result.Diagnostics.Diagnostics)
        {
            var location = diagnostic.Span.HasDocument && !diagnostic.Span.HasUnknownPosition
                ? $"{diagnostic.Span.Document!.Name}:{diagnostic.Span.Start.Line}:{diagnostic.Span.Start.Column}: "
                : string.Empty;

            var color = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => "red",
                DiagnosticSeverity.Warning => "yellow",
                DiagnosticSeverity.Info => "grey",
                _ => "grey",
            };

            _console.MarkupLineInterpolated($"[{color}][[{diagnostic.Severity.ToString().ToUpperInvariant()}]][/] {location}{diagnostic.Message}");
        }

        var exitCode = CompilerExitCode.FromResult(result);

        if (result.HasErrors)
        {
            _console.MarkupLine("[red][[FAILED]][/] Compilation completed with errors.");
            return exitCode;
        }

        _console.MarkupLine("[green][[SUCCESS]][/] Compilation completed without errors.");
        return exitCode;
    }
}
