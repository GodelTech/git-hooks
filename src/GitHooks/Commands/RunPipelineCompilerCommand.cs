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
    private readonly IRunPipelineCompilerHandler _handler = handler;

    /// <inheritdoc/>
    public override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<string>("--file")
        {
            Description = "Path to YAML file (for example: simple.yaml).",
            Required = true,
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var filePath = parseResult.GetValue<string>("--file");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult(1);
        }

        return _handler.HandleAsync(filePath, cancellationToken);
    }
}
