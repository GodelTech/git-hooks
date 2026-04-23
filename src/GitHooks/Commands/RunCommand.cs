using System.CommandLine;

using GitHooks.Handlers;

using Spectre.Console;

namespace GitHooks.Commands;

/// <summary>
/// Command to parse and validate a hook YAML file with repository path context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RunCommand"/> class.
/// </remarks>
/// <param name="runHandler">The handler that processes run command behavior.</param>
/// <param name="console">The console used for command-level validation output.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public sealed class RunCommand(IRunHandler runHandler, IAnsiConsole console)
    : CommandBase("run", "Parse and validate a hook YAML file and display repository path details.")
{
    private readonly IRunHandler _runHandler = runHandler;
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<string>("--file")
        {
            Description = "Path to YAML file (for example: pre-commit.yaml).",
            Required = true,
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var filePath = parseResult.GetValue<string>("--file");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _console.MarkupLine("[red][[ERROR]][/] The [blue]--file[/] option cannot be empty.");
            return Task.FromResult(1);
        }

        return _runHandler.HandleAsync(filePath, cancellationToken);
    }
}
