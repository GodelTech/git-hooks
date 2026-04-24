using GitHooks.Pipeline.Domain;
using GitHooks.Pipeline.Contracts;

using Spectre.Console;

namespace GitHooks.Pipeline.Runner;

/// <summary>
/// Displays pipeline steps for the MVP task runner flow.
/// </summary>
public sealed class StepRunner(IAnsiConsole console) : IStepRunner
{
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public Task<int> RunAsync(PipelineDefinition pipeline, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var shown = 0;
        var skipped = 0;

        for (var index = 0; index < pipeline.Steps.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var step = pipeline.Steps[index];
            var stepNumber = index + 1;

            if (!IsEnabled(step.Enabled))
            {
                skipped++;
                _console.MarkupLineInterpolated($"[yellow][[SKIP]][/] [[{stepNumber}]] [grey]Step #{stepNumber}[/] ({step.Kind})");
                continue;
            }

            shown++;
            var displayName = ResolveDisplayName(step, stepNumber);
            _console.MarkupLineInterpolated($"[green][[STEP]][/] [[{stepNumber}]] [blue]{Markup.Escape(displayName)}[/] ({step.Kind})");
        }

        _console.MarkupLineInterpolated($"[green][[OK]][/] Summary: total [blue]{pipeline.Steps.Count}[/], shown [blue]{shown}[/], skipped [blue]{skipped}[/].");

        return Task.FromResult(0);
    }

    private static bool IsEnabled(string? enabled)
    {
        return enabled switch
        {
            null => true,
            _ when string.IsNullOrWhiteSpace(enabled) => true,
            _ when bool.TryParse(enabled, out var value) => value,
            _ => true,
        };
    }

    private static string ResolveDisplayName(PipelineStep step, int stepNumber)
    {
        return !string.IsNullOrWhiteSpace(step.DisplayName)
            ? step.DisplayName
            : $"Step #{stepNumber}";
    }
}
