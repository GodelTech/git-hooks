using GitHooks.Pipeline.Domain;
using GitHooks.Pipeline.Contracts;

using Spectre.Console;

namespace GitHooks.Pipeline.Runner;

/// <summary>
/// Displays pipeline steps for the task runner flow.
/// </summary>
public sealed class StepRunner(IAnsiConsole console) : IStepRunner
{
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public Task<int> RunAsync(PipelineExecutionPlan plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var shown = 0;
        var skipped = 0;

        for (var index = 0; index < plan.Steps.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var step = plan.Steps[index];
            var stepNumber = step.Order;

            if (!step.Enabled)
            {
                skipped++;
                _console.MarkupLineInterpolated($"[yellow][[SKIP]][/] [[{stepNumber}]] [grey]Step #{stepNumber}[/] ({step.Kind})");
                continue;
            }

            shown++;
            var displayName = ResolveDisplayName(step.DisplayName, stepNumber);
            _console.MarkupLineInterpolated($"[green][[STEP]][/] [[{stepNumber}]] [blue]{Markup.Escape(displayName)}[/] ({step.Kind})");
        }

        _console.MarkupLineInterpolated($"[green][[OK]][/] Summary: total [blue]{plan.Steps.Count}[/], shown [blue]{shown}[/], skipped [blue]{skipped}[/].");

        return Task.FromResult(0);
    }

    private static string ResolveDisplayName(string? displayName, int stepNumber)
    {
        return !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : $"Step #{stepNumber}";
    }
}
