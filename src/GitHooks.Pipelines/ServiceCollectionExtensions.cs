using GitHooks.Pipelines.Contracts;
using GitHooks.Pipelines.Parsing.Yaml;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Pipelines;

/// <summary>
/// Extension methods for registering GitHooks.Pipelines services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all GitHooks.Pipelines services:
    /// <list type="bullet">
    ///   <item><see cref="IYamlPipelineParser"/> → <see cref="YamlPipelineParser"/> (transient)</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPipelines(this IServiceCollection services)
    {
        _ = services.AddTransient<IYamlPipelineParser, YamlPipelineParser>();

        return services;
    }
}
