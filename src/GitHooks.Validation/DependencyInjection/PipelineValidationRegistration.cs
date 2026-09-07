using GitHooks.Compilation.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Validation.DependencyInjection;

/// <summary>
/// Registers the pipeline validation services.
/// </summary>
public static class PipelineValidationRegistration
{
    /// <summary>
    /// Registers <see cref="IPipelineValidator"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddPipelineValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IPipelineValidator, PipelineValidator>();

        return services;
    }
}
