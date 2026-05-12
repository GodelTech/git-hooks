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
    private const string ParameterOptionName = "--parameter";
    private const string ParametersOptionName = "--parameters";

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

        var parameterOption = new Option<string[]>(ParameterOptionName)
        {
            Description = "Parameter override in name=value format. Repeat option to pass multiple values.",
        };
        parameterOption.Aliases.Add("-p");

        yield return parameterOption;

        yield return new Option<string?>(ParametersOptionName)
        {
            Description = "Comma-separated parameter overrides in name=value format.",
        };
    }

    /// <inheritdoc/>
    public override Task<int> HandleActionAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var filePath = parseResult.GetValue<string>("--file");
        var parameterEntries = parseResult.GetValue<string[]>(ParameterOptionName) ?? [];
        var parameterListEntry = parseResult.GetValue<string?>(ParametersOptionName);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            _console.MarkupLine("[red][[ERROR]][/] The [blue]--file[/] option cannot be empty.");
            return Task.FromResult(1);
        }

        if (!TryParseParameterOverrides(parameterEntries, parameterListEntry, out var parameterOverrides, out var errorMessage))
        {
            _console.MarkupLineInterpolated($"[red][[ERROR]][/] {Markup.Escape(errorMessage)}");
            return Task.FromResult(1);
        }

        return _runHandler.HandleAsync(filePath, parameterOverrides, cancellationToken);
    }

    internal static bool TryParseParameterOverrides(
        IReadOnlyList<string> parameterEntries,
        string? parameterListEntry,
        out Dictionary<string, string> parameterOverrides,
        out string errorMessage)
    {
        parameterOverrides = new Dictionary<string, string>(StringComparer.Ordinal);
        errorMessage = string.Empty;

        foreach (var parameterEntry in parameterEntries)
        {
            if (!TryAddParameterOverride(parameterEntry, ParameterOptionName, parameterOverrides, out errorMessage))
            {
                return false;
            }
        }

        if (parameterListEntry is null)
        {
            return true;
        }

        var parameterListItems = parameterListEntry.Split(',', StringSplitOptions.None);

        foreach (var parameterListItem in parameterListItems)
        {
            if (!TryAddParameterOverride(parameterListItem, ParametersOptionName, parameterOverrides, out errorMessage))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryAddParameterOverride(
        string rawEntry,
        string optionName,
        IDictionary<string, string> parameterOverrides,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(rawEntry))
        {
            errorMessage = $"Option '{optionName}' contains an empty parameter entry.";
            return false;
        }

        var separatorIndex = rawEntry.IndexOf('=');

        if (separatorIndex < 0)
        {
            errorMessage = $"Option '{optionName}' entry '{rawEntry}' must use the name=value format.";
            return false;
        }

        var parameterName = rawEntry[..separatorIndex].Trim();

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            errorMessage = $"Option '{optionName}' entry '{rawEntry}' must specify a parameter name before '='.";
            return false;
        }

        var parameterValue = rawEntry[(separatorIndex + 1)..].Trim();

        if (!parameterOverrides.TryAdd(parameterName, parameterValue))
        {
            errorMessage = $"Parameter '{parameterName}' is provided more than once in command-line overrides.";
            return false;
        }

        return true;
    }
}
