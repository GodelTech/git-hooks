namespace GitHooks.Compilation.Expansion;

public interface ITemplateLoader
{
    public ValueTask<SourceContent> LoadAsync(
        TemplateReference reference,
        CancellationToken cancellationToken = default);
}
