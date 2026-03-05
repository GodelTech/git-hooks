using System.CommandLine;

namespace GitHooks.Commands;

public abstract class CommandBase : Command
{
    protected CommandBase(string name, string? description = null)
        : base(name, description)
    {
        foreach (var option in CreateOptions())
        {
            Options.Add(option);
        }

        SetAction(HandleActionAsync);
    }

    protected abstract IEnumerable<Option> CreateOptions();

    protected abstract Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken token);
}
