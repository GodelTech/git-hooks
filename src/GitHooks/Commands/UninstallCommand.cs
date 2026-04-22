using System.CommandLine;

using GitHooks.Handlers;
using GitHooks.Infrastructure.Git;

namespace GitHooks.Commands;

/// <summary>
/// Command to uninstall git hooks by removing the git core.hooksPath setting.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UninstallCommand"/> class.
/// </remarks>
/// <param name="uninstallHandler">The handler that performs hook uninstallation.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public sealed class UninstallCommand(IUninstallHandler uninstallHandler)
    : CommandBase("uninstall", "Remove the git core.hooksPath configuration.")
{
    private readonly IUninstallHandler _uninstallHandler = uninstallHandler;

    /// <inheritdoc/>
    public override IEnumerable<Option> CreateOptions()
    {
        yield return new Option<GitConfigScope>(
            "--scope"
        )
        {
            Description = "Git configuration scope to use (local, global, or system). Default: global",
            DefaultValueFactory = _ => GitHooksDefaults.Scope
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var scope = parseResult.GetValue<GitConfigScope>("--scope");

        return _uninstallHandler.HandleAsync(scope, cancellationToken);
    }
}
