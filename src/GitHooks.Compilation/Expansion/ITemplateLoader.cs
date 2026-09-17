namespace GitHooks.Compilation.Expansion;

public interface ITemplateLoader
{
    public SourceContent Load(
        TemplateReference reference);

    public ValueTask<SourceContent> LoadAsync(
        TemplateReference reference,
        CancellationToken cancellationToken = default);
}
