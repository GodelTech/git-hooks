namespace GitHooks.Templates;

/// <summary>
/// Validates scaffold template content after token substitution.
/// </summary>
public interface ITemplateTokenValidator
{
    /// <summary>
    /// Throws when unresolved placeholders remain in rendered template content.
    /// </summary>
    /// <param name="content">The rendered content to validate.</param>
    public void ValidateNoUnresolvedTokens(string content);
}
