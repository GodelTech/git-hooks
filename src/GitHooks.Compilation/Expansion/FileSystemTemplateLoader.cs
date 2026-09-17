using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Expansion;

internal sealed class FileSystemTemplateLoader : ITemplateLoader
{
    public SourceContent Load(
        TemplateReference reference)
    {
        var text = File.ReadAllText(reference.Path);

        return new SourceContent(
            text,
            new SourceDocument(reference.Path));
    }

    public ValueTask<SourceContent> LoadAsync(
        TemplateReference reference,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<SourceContent>(
            Load(reference));
    }
}
