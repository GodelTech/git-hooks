using GitHooks.Infrastructure.Hooks;
using GitHooks.Templates;

namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Provides Bash hook template content from embedded resources.
/// </summary>
public sealed class BashTemplateProvider(ITemplateTokenValidator tokenValidator, IToolVersionResolver toolVersionResolver)
    : EmbeddedTemplateProviderBase("GitHooks.Infrastructure.Scaffold.Templates.hook.sh"), IBashTemplateProvider
{
    private readonly ITemplateTokenValidator _tokenValidator = tokenValidator;
    private readonly IToolVersionResolver _toolVersionResolver = toolVersionResolver;

    /// <inheritdoc/>
    public string ApplyTokens(string templateContent, string hooksPath, GitHook hook)
    {
        var result = templateContent
            .Replace(TemplateTokens.HookName, hook.Name)
            .Replace(TemplateTokens.HookDescription, hook.Description)
            .Replace(TemplateTokens.HooksPath, hooksPath)
            .Replace(TemplateTokens.Version, _toolVersionResolver.ResolveVersion());

        _tokenValidator.ValidateNoUnresolvedTokens(result);

        return result;
    }
}

