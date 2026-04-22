using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.GitHooks;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Infrastructure;

/// <summary>
/// Extension methods for registering GitHooks.Infrastructure services with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all GitHooks.Infrastructure services:
    /// <list type="bullet">
    ///   <item><see cref="ICommandLine"/> → <see cref="CommandLine"/> (transient)</item>
    ///   <item><see cref="IGitCommandLine"/> → <see cref="GitCommandLine"/> (transient)</item>
    ///   <item><see cref="IHookFileManager"/> → <see cref="HookFileManager"/> (transient)</item>
    ///   <item><see cref="IGitHookCatalog"/> → <see cref="GitHookCatalog"/> (singleton)</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddGitHooksInfrastructure(this IServiceCollection services)
    {
        _ = services.AddTransient<ICommandLine, CommandLine>();
        _ = services.AddTransient<IGitCommandLine, GitCommandLine>();
        _ = services.AddTransient<IHookFileManager, HookFileManager>();
        _ = services.AddSingleton<IGitHookCatalog, GitHookCatalog>();

        return services;
    }
}
