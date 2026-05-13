using GitHooks.Infrastructure.Hooks;
using GitHooks.Templates;

namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Provides YAML hook template content from embedded resources.
/// </summary>
public sealed class YamlTemplateProvider(ITemplateTokenValidator tokenValidator, IToolVersionResolver toolVersionResolver)
    : EmbeddedTemplateProviderBase("GitHooks.Infrastructure.Scaffold.Templates.hook.yaml"), IYamlTemplateProvider
{
    private readonly ITemplateTokenValidator _tokenValidator = tokenValidator;
    private readonly IToolVersionResolver _toolVersionResolver = toolVersionResolver;

    /// <inheritdoc/>
    public string ApplyTokens(string templateContent, GitHook hook)
    {
        ArgumentNullException.ThrowIfNull(templateContent);
        ArgumentNullException.ThrowIfNull(hook);

        var result = templateContent
            .Replace(TemplateTokens.HookName, hook.Name)
            .Replace(TemplateTokens.HookDescription, hook.Description)
            .Replace(TemplateTokens.Version, _toolVersionResolver.ResolveVersion());

        _tokenValidator.ValidateNoUnresolvedTokens(result);

        return result;
    }
}
