using System.CommandLine;

using GitHooks.Handlers;
using GitHooks.Infrastructure.Git;

namespace GitHooks.Commands;

/// <summary>
/// Command to create git hook scaffold files in a hooks directory.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateCommand"/> class.
/// </remarks>
/// <param name="createHookHandler">The handler that creates hook scaffold files.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public sealed class CreateCommand(ICreateHookHandler createHookHandler)
    : CommandBase("create", $"Create git hook scaffold files in the {GitHooksDefaults.HooksPath}/ directory.")
{
    private readonly ICreateHookHandler _createHookHandler = createHookHandler;

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

        yield return new Option<string>(
            "--hooks-path"
        )
        {
            Description = $"Directory where hook scaffold files are created. Default: {GitHooksDefaults.HooksPath}",
            DefaultValueFactory = _ => GitHooksDefaults.HooksPath
        };

        yield return new Option<string[]>(
            "--hooks"
        )
        {
            Description = $"Hook names to create (repeat option or pass comma-separated values). Default: {string.Join(", ", GitHooksDefaults.CreateHooks)}",
            AllowMultipleArgumentsPerToken = true,
            CustomParser = result => [.. result.Tokens.SelectMany(t => t.Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))],
            DefaultValueFactory = _ => [.. GitHooksDefaults.CreateHooks]
        };

        yield return new Option<bool>(
            "--force"
        )
        {
            Description = "Overwrite existing hook files.",
            DefaultValueFactory = _ => false
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var scope = parseResult.GetValue<GitConfigScope>("--scope");
        var hooksPath = parseResult.GetValue<string>("--hooks-path") ?? GitHooksDefaults.HooksPath;
        var hooks = parseResult.GetValue<string[]>("--hooks");

        if (hooks is null || hooks.Length == 0)
        {
            hooks = [.. GitHooksDefaults.CreateHooks];
        }

        var force = parseResult.GetValue<bool>("--force");

        return _createHookHandler.HandleAsync(scope, hooksPath, hooks, force, cancellationToken);
    }
}


