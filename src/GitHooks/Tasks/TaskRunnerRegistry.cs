using GitHooks.Pipelines.Models;

namespace GitHooks.Tasks;

public sealed class TaskRunnerRegistry
{
    private readonly IReadOnlyList<ITaskRunner> _runners;

    public TaskRunnerRegistry(IEnumerable<ITaskRunner>? runners = null)
    {
        _runners = runners?.ToList() ?? [new BashTaskRunner(), new DotnetTaskRunner()];
    }

    public ITaskRunner? Resolve(string taskName) =>
        _runners.FirstOrDefault(r => r.CanHandle(taskName));

    public async Task<int> RunAsync(Step step, string[] hookArgs, CancellationToken cancellationToken = default)
    {
        var runner = Resolve(step.Task);

        if (runner is null)
        {
            Console.Error.WriteLine($"[warning] Unknown task type '{step.Task}' in step '{step.Name ?? step.Task}'. Skipping.");
            return 1;
        }

        return await runner.RunAsync(step, hookArgs, cancellationToken);
    }
}
