using GitHooks.Infrastructure.Templates;

namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Provides Bash hook template content from embedded resources.
/// </summary>
public sealed class BashTemplateProvider()
    : EmbeddedTemplateProviderBase("GitHooks.Infrastructure.Scaffold.Templates.hook.sh"), IBashTemplateProvider
{

}
