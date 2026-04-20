using System.CommandLine;

using GitHooks.Handlers;

namespace GitHooks.Commands;

/// <summary>
/// Command to uninstall git hooks by removing the git core.hooksPath setting.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UninstallCommand"/> class.
/// </remarks>
/// <param name="uninstallHandler">The handler that performs hook uninstallation.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public class UninstallCommand(IUninstallHandler uninstallHandler)
    : CommandBase("uninstall", "Remove the git core.hooksPath configuration.")
{
    private const string DefaultScope = "global";

    private readonly IUninstallHandler _uninstallHandler = uninstallHandler;

    /// <inheritdoc/>
    public override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<string>(
            "--scope"
        )
        {
            Description = "Git configuration scope to use (global or system). Default: global",
            DefaultValueFactory = _ => DefaultScope
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var scope = parseResult.GetValue<string>("--scope") ?? DefaultScope;

        return _uninstallHandler.HandleAsync(scope, cancellationToken);
    }
}
