using System.CommandLine;

using GitHooks.Handlers;
using GitHooks.Infrastructure.Git;

namespace GitHooks.Commands;

/// <summary>
/// Command to create git hook files in a hooks directory.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateCommand"/> class.
/// </remarks>
/// <param name="createHookHandler">The handler that creates hook files.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherited from System.CommandLine.Command base class")]
public sealed class CreateCommand(ICreateHookHandler createHookHandler)
    : CommandBase("create", $"Create git hook files in the {GitHooksDefaults.HooksPath}/ directory.")
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
            Description = $"Directory where hook files are created. Default: {GitHooksDefaults.HooksPath}",
            DefaultValueFactory = _ => GitHooksDefaults.HooksPath
        };

        yield return new Option<string[]>(
            "--hooks"
        )
        {
            Description = "Hook names to create (repeat option or pass comma-separated values).",
            AllowMultipleArgumentsPerToken = true,
            DefaultValueFactory = _ => []
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
        var rawHooks = parseResult.GetValue<string[]>("--hooks") ?? [];
        var force = parseResult.GetValue<bool>("--force");

        var hooks = ResolveHooks(rawHooks);

        return _createHookHandler.HandleAsync(scope, hooksPath, hooks, force, cancellationToken);
    }

    private static string[] ResolveHooks(IEnumerable<string> rawHooks)
    {
        var hooks = rawHooks
            .SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        return hooks.Length > 0 ? hooks : [.. GitHooksDefaults.CommitHooks, .. GitHooksDefaults.PushHooks];
    }
}
