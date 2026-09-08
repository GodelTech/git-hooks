using System.CommandLine;

using GitHooks.Handlers;

namespace GitHooks.Commands;

/// <summary>
/// TEMPORARY: manual verification command for <see cref="Compilation.PipelineCompiler"/>.
/// Not intended to ship long-term; used to visually confirm the
/// YAML -> Parser -> Domain AST -> Template Expansion -> Validation flow by
/// running the real compiler against a YAML file and printing the result.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RunPipelineCompilerCommand"/> class.
/// </remarks>
/// <param name="handler">The handler that processes the run-pipeline-compiler command behavior.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public sealed class RunPipelineCompilerCommand(IRunPipelineCompilerHandler handler)
    : CommandBase("run-pipeline-compiler", "TEMPORARY: compile a YAML file with the target PipelineCompiler and print the AST/diagnostics.")
{
    private const string ParameterOptionName = "--parameter";
    private const string ParametersOptionName = "--parameters";

    private readonly IRunPipelineCompilerHandler _handler = handler;

    /// <inheritdoc/>
    public override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<string>("--file")
        {
            Description = "Path to YAML file (for example: simple.yaml).",
            Required = true,
        };

        var parameterOption = new Option<string[]>(ParameterOptionName)
        {
            Description = "Parameter override in name=value format. Repeat option to pass multiple values.",
        };
        parameterOption.Aliases.Add("-p");

        yield return parameterOption;

        yield return new Option<string?>(ParametersOptionName)
        {
            Description = "Comma-separated parameter overrides in name=value format.",
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var filePath = parseResult.GetValue<string>("--file");
        var parameterEntries = parseResult.GetValue<string[]>(ParameterOptionName) ?? [];
        var parameterListEntry = parseResult.GetValue<string?>(ParametersOptionName);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult(1);
        }

        if (!RunCommand.TryParseParameterOverrides(parameterEntries, parameterListEntry, out var parameterOverrides, out _))
        {
            return Task.FromResult(1);
        }

        return _handler.HandleAsync(filePath, parameterOverrides, cancellationToken);
    }
}
