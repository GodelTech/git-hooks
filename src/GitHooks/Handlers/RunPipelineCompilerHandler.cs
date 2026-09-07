using GitHooks.Compilation;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

using Spectre.Console;

namespace GitHooks.Handlers;

/// <summary>
/// TEMPORARY: manual verification handler for <see cref="PipelineCompiler"/>.
/// Not intended to ship long-term; used to visually confirm the
/// YAML -> Parser -> Domain AST -> Template Expansion -> Validation flow.
/// </summary>
public sealed class RunPipelineCompilerHandler(
    PipelineCompiler compiler,
    IAnsiConsole console)
    : IRunPipelineCompilerHandler
{
    private readonly PipelineCompiler _compiler = compiler;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> HandleAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var absoluteFilePath = Path.GetFullPath(filePath);

        if (!File.Exists(absoluteFilePath))
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] YAML file not found: [blue]{Markup.Escape(absoluteFilePath)}[/]");
            return 1;
        }

        var yamlText = await File.ReadAllTextAsync(absoluteFilePath, cancellationToken);
        var document = new SourceDocument(Path.GetFileName(absoluteFilePath));

        var result = _compiler.Compile(
            yamlText,
            document);

        _console.MarkupLine("[bold]== Domain AST ==[/]");
        _console.MarkupLineInterpolated($"[blue]AST root type:[/] {result.Root.GetType().FullName}");
        _console.MarkupLineInterpolated($"[blue]Root kind:[/] {result.Root.Kind}");

        _console.MarkupLineInterpolated($"[blue]Parameters ({result.Root.Parameters.Count}):[/]");
        foreach (var parameter in result.Root.Parameters)
        {
            var name = DescribeExpression(parameter.Name.Value);
            var type = parameter.Type is null ? "(none)" : DescribeExpression(parameter.Type.Value);
            var defaultValue = parameter.DefaultValue is null ? "(none)" : DescribeExpression(parameter.DefaultValue.Value);

            _console.MarkupLineInterpolated($"  - name: {Markup.Escape(name)}, type: {Markup.Escape(type)}, default: {Markup.Escape(defaultValue)}");
        }

        _console.MarkupLineInterpolated($"[blue]Steps ({result.Root.Steps.Count}):[/]");
        foreach (var step in result.Root.Steps)
        {
            _console.MarkupLineInterpolated($"  - {Markup.Escape(DescribeStep(step))}");
        }

        _console.MarkupLine("[bold]== Diagnostics ==[/]");

        foreach (var diagnostic in result.Diagnostics.Diagnostics)
        {
            var location = diagnostic.Span.HasDocument && !diagnostic.Span.HasUnknownPosition
                ? $"{Markup.Escape(diagnostic.Span.Document!.Name)}:{diagnostic.Span.Start.Line}:{diagnostic.Span.Start.Column}: "
                : string.Empty;

            var color = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => "red",
                DiagnosticSeverity.Warning => "yellow",
                DiagnosticSeverity.Info => "grey",
                _ => "grey",
            };

            _console.MarkupLineInterpolated($"[{color}][[{diagnostic.Severity.ToString().ToUpperInvariant()}]][/] {location}{Markup.Escape(diagnostic.Message)}");
        }

        if (result.Diagnostics.Count == 0)
        {
            _console.MarkupLine("[grey](no diagnostics reported)[/]");
        }

        if (result.Diagnostics.HasErrors)
        {
            _console.MarkupLine("[red][[FAILED]][/] Compilation completed with errors.");
            return 1;
        }

        _console.MarkupLine("[green][[SUCCESS]][/] Compilation completed without errors.");
        return 0;
    }

    private static string DescribeExpression(
        ExpressionNode expression)
    {
        return expression switch
        {
            StringLiteralExpressionNode stringLiteral => $"\"{stringLiteral.Value}\"",
            BooleanLiteralExpressionNode booleanLiteral => booleanLiteral.Value.ToString(),
            IntegerLiteralExpressionNode integerLiteral => integerLiteral.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ParameterVariableExpressionNode parameterVariable => $"parameters.{parameterVariable.Name}",
            InvalidVariableExpressionNode => "(invalid variable)",
            InterpolatedStringExpressionNode => "(interpolated string)",
            _ => expression.Kind.ToString(),
        };
    }

    private static string DescribeStep(
        StepNode step)
    {
        return step switch
        {
            ScriptStepNode scriptStep =>
                $"script: {DescribeExpression(scriptStep.Script.Value)}"
                + (scriptStep.DisplayName is null ? string.Empty : $" (displayName: {DescribeExpression(scriptStep.DisplayName.Value)})"),
            TemplateStepNode templateStep =>
                $"template: {DescribeExpression(templateStep.Template.Value)}",
            InvalidStepNode invalidStep =>
                $"invalid step ({invalidStep.Fields.Count} unknown field(s))",
            _ => step.Kind.ToString(),
        };
    }
}
