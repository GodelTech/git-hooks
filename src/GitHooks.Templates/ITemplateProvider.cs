namespace GitHooks.Templates;

/// <summary>
/// Provides template content to consumers.
/// </summary>
public interface ITemplateProvider
{
    /// <summary>
    /// Retrieves template text.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>A task containing the template content.</returns>
    public ValueTask<string> GetTemplateAsync(CancellationToken cancellationToken = default);
}

