using System.CommandLine;
using GitHooks.Services;
using Spectre.Console;

namespace GitHooks.Commands;

public class TempCommand : CommandBase
{
    private const string DefaultHooksPath = ".githooks";

    private readonly IInstallService _installService;
    private readonly IAnsiConsole _console;

    public TempCommand(IInstallService installService, IAnsiConsole console)
        : base("install", $"Write hook scripts into {DefaultHooksPath}/ and configure git to use that directory.")
    {
        _installService = installService;
        _console = console;
    }

    protected override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<string>(
            "--hooks-path"
        )
        {
            Description = $"Git global core.hooksPath value to setup. Default: {DefaultHooksPath}",
            DefaultValueFactory = _ => DefaultHooksPath
        };
        yield return new Option<bool>(
            "--force"
        )
        {
            Description = "Force installation by overwriting existing git global core.hooksPath without prompting.",
            DefaultValueFactory = _ => false
        };
    }

    protected override async Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var hooksPath = parseResult.GetValue<string>("--hooks-path");
        var force = parseResult.GetValue<bool>("--force");

        _console.MarkupLine($"Using hooks path: [blue]{hooksPath}[/]");

        var result = await _installService.RunAsync(hooksPath, force, cancellationToken);

        return result.ExitCode;
    }
}
