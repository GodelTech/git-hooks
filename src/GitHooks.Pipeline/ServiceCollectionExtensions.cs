using GitHooks.Pipeline.Execution;
using GitHooks.Pipeline.Parsing;
using GitHooks.Pipeline.Transformation;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Pipeline;

/// <summary>
/// Extension methods for registering GitHooks.Pipeline services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers parser, transformer, and runner services for pipeline processing.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        _ = services.AddTransient<IPipelineParser, PipelineParser>();
        _ = services.AddTransient<ITemplateExpander, TemplateExpander>();
        _ = services.AddTransient<IPipelineRunner, PipelineRunner>();

        return services;
    }
}
