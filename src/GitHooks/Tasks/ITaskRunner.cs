using GitHooks.Pipelines.Models;

namespace GitHooks.Tasks;

public interface ITaskRunner
{
    bool CanHandle(string taskName);
    Task<int> RunAsync(Step step, string[] hookArgs, CancellationToken cancellationToken = default);
}
