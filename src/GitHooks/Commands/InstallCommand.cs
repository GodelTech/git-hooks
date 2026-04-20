using System.CommandLine;

using GitHooks.CommandLine;
using GitHooks.Handlers;

namespace GitHooks.Commands;

/// <summary>
/// Command to install git hooks by configuring the git core.hooksPath setting.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InstallCommand"/> class.
/// </remarks>
/// <param name="installHandler">The handler that performs hook installation.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public class InstallCommand(IInstallHandler installHandler)
    : CommandBase("install", $"Configure git core.hooksPath to use {DefaultHooksPath}/ directory.")
{
    private const GitConfigScope DefaultScope = GitConfigScope.Global;
    private const string DefaultHooksPath = ".githooks";

    private readonly IInstallHandler _installHandler = installHandler;

    /// <inheritdoc/>
    public override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<GitConfigScope>(
            "--scope"
        )
        {
            Description = "Git configuration scope to use (local, global, or system). Default: global",
            DefaultValueFactory = _ => DefaultScope
        };

        yield return new Option<string>(
            "--hooks-path"
        )
        {
            Description = $"Git core.hooksPath value to setup. Default: {DefaultHooksPath}",
            DefaultValueFactory = _ => DefaultHooksPath
        };
        yield return new Option<bool>(
            "--force"
        )
        {
            Description = "Force installation by overwriting existing core.hooksPath without prompting.",
            DefaultValueFactory = _ => false
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var scope = parseResult.GetValue<GitConfigScope>("--scope");
        var hooksPath = parseResult.GetValue<string>("--hooks-path") ?? DefaultHooksPath;
        var force = parseResult.GetValue<bool>("--force");

        return _installHandler.HandleAsync(scope, hooksPath, force, cancellationToken);
    }
}
