using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.IO;

internal sealed class LocalPipelineSourceContentReader
    : IPipelineSourceContentReader
{
    public bool CanRead(PipelineSource source)
    {
        return source.Kind is PipelineSourceKind.LocalFile;
    }

    public async Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(source.Identifier, cancellationToken);
    }
}
