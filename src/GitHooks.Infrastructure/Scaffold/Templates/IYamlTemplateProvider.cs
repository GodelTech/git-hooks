using GitHooks.Infrastructure.Hooks;
using GitHooks.Templates;

namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Marker abstraction for services that provide YAML hook templates.
/// </summary>
public interface IYamlTemplateProvider
    : ITemplateProvider
{
    /// <summary>
    /// Applies token replacements to a YAML hook template.
    /// </summary>
    /// <param name="templateContent">The raw template content to transform.</param>
    /// <param name="hook">The target hook metadata used for token substitution.</param>
    /// <returns>The tokenized YAML hook content.</returns>
    public string ApplyTokens(string templateContent, GitHook hook);
}
