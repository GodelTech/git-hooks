using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.IO;

public sealed class PipelineContentReader(IEnumerable<IPipelineSourceContentReader> readers)
    : IPipelineContentReader
{
    private readonly IReadOnlyList<IPipelineSourceContentReader> _readers = [.. readers];

    public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
    {
        var reader = _readers.FirstOrDefault(reader => reader.CanRead(source));

        return reader is not null
            ? reader.ReadAsync(source, cancellationToken)
            : Task.FromException<string>(
                new InvalidOperationException($"No pipeline content reader registered for source kind '{source.Kind}'.")
            );
    }
}
