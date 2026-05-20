using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.IO;

internal sealed class LocalPipelineContentReader : IPipelineContentSourceReader
{
    public bool CanRead(PipelineSource source)
    {
        return source.Kind is PipelineSourceKind.LocalFile;
    }

    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
    {
        return File.ReadAllTextAsync(source.Identifier, cancellationToken);
    }
}
