using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Hooks;
using GitHooks.Infrastructure.Scaffold;
using GitHooks.Infrastructure.Scaffold.Templates;

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
    ///   <item><see cref="IGitHookCatalog"/> → <see cref="GitHookCatalog"/> (singleton)</item>
    ///   <item><see cref="IBashTemplateProvider"/> → <see cref="BashTemplateProvider"/> (singleton)</item>
    ///   <item><see cref="IYamlTemplateProvider"/> → <see cref="YamlTemplateProvider"/> (singleton)</item>
    ///   <item><see cref="IGitHookScaffolder"/> → <see cref="GitHookScaffolder"/> (transient)</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        _ = services.AddTransient<ICommandLine, CommandLine>();
        _ = services.AddTransient<IGitCommandLine, GitCommandLine>();
        _ = services.AddSingleton<IGitHookCatalog, GitHookCatalog>();
        _ = services.AddSingleton<IBashTemplateProvider, BashTemplateProvider>();
        _ = services.AddSingleton<IYamlTemplateProvider, YamlTemplateProvider>();
        _ = services.AddTransient<IGitHookScaffolder, GitHookScaffolder>();

        return services;
    }
}

