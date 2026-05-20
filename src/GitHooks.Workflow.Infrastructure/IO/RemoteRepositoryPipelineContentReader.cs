using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.IO;

internal sealed class RemoteRepositoryPipelineContentReader : IPipelineContentSourceReader
{
    public bool CanRead(PipelineSource source)
    {
        return source.Kind is PipelineSourceKind.RemoteRepository;
    }

    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
    {
        return Task.FromException<string>(new NotSupportedException("Remote repository pipeline content reading is not implemented yet."));
    }
}
