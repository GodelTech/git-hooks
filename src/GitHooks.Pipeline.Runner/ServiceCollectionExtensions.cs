using GitHooks.Pipeline.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Pipeline.Runner;

/// <summary>
/// Extension methods for registering GitHooks.Pipeline.Runner services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all GitHooks.Pipeline.Runner services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPipelineRunner(this IServiceCollection services)
    {
        _ = services.AddTransient<IStepRunner, StepRunner>();

        return services;
    }
}
