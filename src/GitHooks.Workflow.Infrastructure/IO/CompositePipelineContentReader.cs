using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.IO;

internal sealed class CompositePipelineContentReader(IEnumerable<IPipelineContentSourceReader> readers) : IPipelineContentReader
{
    private readonly IReadOnlyList<IPipelineContentSourceReader> _readers = [.. readers];

    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
    {
        var reader = _readers.FirstOrDefault(candidate => candidate.CanRead(source));

        return reader is not null
            ? reader.ReadAsync(source, cancellationToken)
            : Task.FromException<string>(new InvalidOperationException($"No pipeline content reader registered for source kind '{source.Kind}'."));
    }
}
