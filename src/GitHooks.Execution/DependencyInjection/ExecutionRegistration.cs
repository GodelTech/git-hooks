using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Execution.DependencyInjection;

/// <summary>
/// Registers the pipeline execution services.
/// </summary>
public static class ExecutionRegistration
{
    /// <summary>
    /// Registers the run handler and script step runner.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddExecution(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddTransient<IRunHandler, RunHandler>();
        _ = services.AddTransient<IScriptStepRunner, ShellScriptStepRunner>();

        return services;
    }
}
