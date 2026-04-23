using GitHooks.Infrastructure.Hooks;
using GitHooks.Templates;

namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Marker abstraction for services that provide Bash hook templates.
/// </summary>
public interface IBashTemplateProvider : ITemplateProvider
{
    /// <summary>
    /// Applies token replacements to a Bash hook template.
    /// </summary>
    /// <param name="templateContent">The raw template content to transform.</param>
    /// <param name="hooksPath">The configured hooks path used for token substitution.</param>
    /// <param name="hook">The target hook metadata used for token substitution.</param>
    /// <returns>The tokenized Bash hook content.</returns>
    public string ApplyTokens(string templateContent, string hooksPath, GitHook hook);
}
