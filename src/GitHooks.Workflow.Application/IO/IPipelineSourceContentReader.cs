using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.IO;

public interface IPipelineSourceContentReader
{
    public bool CanRead(PipelineSource source);

    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default);
}
