using GitHooks.Workflow.Infrastructure.Yaml;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.DependencyInjection;

public static class YamlParsingRegistration
{
    public static IServiceCollection AddYamlPipelineParsing(
        this IServiceCollection services)
    {
        _ = services.AddSingleton<IYamlParserFactory, YamlParserFactory>();

        _ = services.AddSingleton(
            provider =>
            {
                var yamlFactory = provider.GetRequiredService<IYamlParserFactory>();

                return PipelineParserFactory.Create(yamlFactory);
            }
        );

        return services;
    }
}
