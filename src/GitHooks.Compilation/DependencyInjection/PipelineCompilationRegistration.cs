using GitHooks.Compilation.Binding;
using GitHooks.Compilation.Expansion;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Compilation.DependencyInjection;

/// <summary>
/// Registers the services required to compile a pipeline from a parsed AST through
/// template expansion, producing a <see cref="PipelineCompiler"/> ready for use.
/// </summary>
public static class PipelineCompilationRegistration
{
    /// <summary>
    /// Registers <see cref="ITemplateExpander"/>, <see cref="IPipelineAstCompiler"/>,
    /// <see cref="IParameterBinder"/>, and <see cref="PipelineCompiler"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddTargetPipelineCompilation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<ITemplateExpander, TemplateExpander>();
        _ = services.AddSingleton<IPipelineAstCompiler, PipelineAstCompiler>();
        _ = services.AddSingleton<IParameterBinder, ParameterBinder>();
        _ = services.AddSingleton<PipelineCompiler>();

        return services;
    }
}
