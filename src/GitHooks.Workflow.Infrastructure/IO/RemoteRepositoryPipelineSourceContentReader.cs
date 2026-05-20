using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.IO;

internal sealed class RemoteRepositoryPipelineSourceContentReader
    : IPipelineSourceContentReader
{
    public bool CanRead(PipelineSource source)
    {
        return source.Kind is PipelineSourceKind.RemoteRepository;
    }

    public async Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
    {
        return await Task.FromException<string>(
            new NotSupportedException("Remote repository pipeline content reading is not implemented yet.")
        );
    }
}
