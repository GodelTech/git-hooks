using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Templates;

/// <summary>
/// Extension methods for registering GitHooks.Templates services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all GitHooks.Templates services:
    /// <list type="bullet">
    ///   <item><see cref="IToolVersionResolver"/> → <see cref="ToolVersionResolver"/> (singleton)</item>
    ///   <item><see cref="ITemplateTokenValidator"/> → <see cref="TemplateTokenValidator"/> (singleton)</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddTemplates(this IServiceCollection services)
    {
        _ = services.AddSingleton<IToolVersionResolver, ToolVersionResolver>();
        _ = services.AddSingleton<ITemplateTokenValidator, TemplateTokenValidator>();

        return services;
    }
}
