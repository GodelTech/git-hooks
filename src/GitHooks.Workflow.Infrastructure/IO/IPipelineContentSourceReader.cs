using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.IO;

internal interface IPipelineContentSourceReader
{
    public bool CanRead(PipelineSource source);

    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default);
}
