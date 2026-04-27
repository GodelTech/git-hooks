using GitHooks.Pipeline.Compilation;
using GitHooks.Pipeline.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Pipeline;

/// <summary>
/// Extension methods for registering GitHooks.Pipeline core services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers GitHooks.Pipeline core services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        _ = services.AddTransient<IPipelineCompiler, PipelineCompiler>();

        return services;
    }
}
