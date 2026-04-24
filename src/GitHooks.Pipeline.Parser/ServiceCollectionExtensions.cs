using GitHooks.Pipeline.Contracts;
using GitHooks.Pipeline.Parser.Parsing.Yaml;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Pipeline.Parser;

/// <summary>
/// Extension methods for registering GitHooks.Pipeline.Parser services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers parser services:
    /// <list type="bullet">
    ///   <item><see cref="IYamlPipelineParser"/> -> <see cref="YamlPipelineParser"/> (transient)</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPipelineParser(this IServiceCollection services)
    {
        _ = services.AddTransient<IYamlPipelineParser, YamlPipelineParser>();

        return services;
    }
}
