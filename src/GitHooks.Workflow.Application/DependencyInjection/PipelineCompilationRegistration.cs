using GitHooks.Workflow.Application.Compilation;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.DependencyInjection;

public static class PipelineCompilationRegistration
{
    public static IServiceCollection AddPipelineCompilation(this IServiceCollection services)
    {
        _ = services.AddSingleton<IPipelineCompiler, PipelineCompiler>();

        return services;
    }
}
