using GitHooks.Compilation;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Execution;
using GitHooks.Infrastructure.Git;

using Spectre.Console;

namespace GitHooks.Handlers;

public sealed class RunHandler(
    IGitCommandLine gitCommandLine,
    PipelineCompiler compiler,
    IAnsiConsole console,
    IScriptStepRunner stepRunner)
    : IRunHandler
{
    private readonly IGitCommandLine _gitCommandLine = gitCommandLine;
    private readonly PipelineCompiler _compiler = compiler;
    private readonly IAnsiConsole _console = console;
    private readonly IScriptStepRunner _stepRunner = stepRunner;

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

        return await ExecuteStepsAsync(result, repositoryRootPath, cancellationToken);
    }

    private async Task<int> ExecuteStepsAsync(
        CompilationResult result,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var steps = result.BoundRoot.Steps;

        var plan = new List<(string Name, string Script)>(steps.Count);

        for (var i = 0; i < steps.Count; i++)
        {
            var position = $"{i + 1}/{steps.Count}";

            if (steps[i] is not ScriptStepNode scriptStep)
            {
                _console.MarkupLineInterpolated($"[red][[ERROR]][/] Step {position}: unsupported step kind '{steps[i].Kind}'; refusing to run a pipeline with steps that would be skipped.");
                return 1;
            }

            if (!scriptStep.Script.Value.TryGetStringValue(out var script))
            {
                _console.MarkupLineInterpolated($"[red][[ERROR]][/] Step {position}: script contains unresolved expressions.");
                return 1;
            }

            WarnAboutIgnoredFields(scriptStep, position);

            var name = scriptStep.DisplayName is not null && scriptStep.DisplayName.Value.TryGetStringValue(out var displayName)
                ? displayName
                : script.Split('\n', 2)[0].Trim();

            plan.Add((name, script));
        }

        for (var i = 0; i < plan.Count; i++)
        {
            var position = $"{i + 1}/{plan.Count}";

            _console.MarkupLineInterpolated($"[blue]==> Step {position}:[/] {plan[i].Name}");

            int stepExitCode;

            try
            {
                stepExitCode = await _stepRunner.RunAsync(plan[i].Script, workingDirectory, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or InvalidOperationException or System.ComponentModel.Win32Exception or UnauthorizedAccessException)
            {
                _console.MarkupLineInterpolated($"[red][[ERROR]][/] Step {position} could not be run: {exception.Message}");
                return 1;
            }

            if (stepExitCode != 0)
            {
                _console.MarkupLineInterpolated($"[red][[FAILED]][/] Step {position} exited with code {stepExitCode}.");
                return stepExitCode;
            }
        }

        _console.MarkupLine("[green][[SUCCESS]][/] All steps completed.");
        return 0;
    }

    private void WarnAboutIgnoredFields(
        ScriptStepNode step,
        string position)
    {
        var ignored = new List<string>();

        if (step.Condition is not null)
        {
            ignored.Add(step.Condition.Key);
        }

        if (step.Env is not null)
        {
            ignored.Add(step.Env.Key);
        }

        if (step.WorkingDirectory is not null)
        {
            ignored.Add(step.WorkingDirectory.Key);
        }

        if (step.TimeoutInMinutes is not null)
        {
            ignored.Add(step.TimeoutInMinutes.Key);
        }

        if (ignored.Count > 0)
        {
            _console.MarkupLineInterpolated($"[yellow][[WARNING]][/] Step {position}: field(s) not supported yet and ignored: {string.Join(", ", ignored)}.");
        }
    }
}
