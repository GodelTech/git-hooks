using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace GitHooks.Commands;

/// <summary>
/// Base class for custom commands.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandBase"/> class.
/// </remarks>
/// <param name="name">The name of the command.</param>
/// <param name="description">The description of the command.</param>
[SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Inherits IEnumerable from Command base class for internal command/option management, not intended as a collection type")]
public abstract class CommandBase(string name, string? description = null)
    : Command(name, description)
{
    /// <summary>
    /// Creates the options for this command.
    /// </summary>
    /// <returns>An enumerable collection of options.</returns>
    public abstract IEnumerable<Option> CreateOptions();

    /// <summary>
    /// Handles the command action asynchronously.
    /// </summary>
    /// <param name="parseResult">The parse result containing command line arguments.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with an exit code.</returns>
    public abstract Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken);
}
