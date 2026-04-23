using GitHooks.Infrastructure.Templates;

namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Provides YAML hook template content from embedded resources.
/// </summary>
public sealed class YamlTemplateProvider()
    : EmbeddedTemplateProviderBase("GitHooks.Infrastructure.Scaffold.Templates.hook.yaml"), IYamlTemplateProvider
{

}
