using GitHooks.Workflow.Application.Resolution;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.DependencyInjection;

public static class PipelineResolutionRegistration
{
    public static IServiceCollection AddPipelineResolution(this IServiceCollection services)
    {
        _ = services.AddPipelineTemplateExpansion();
        _ = services.AddSingleton<IPipelineResolver, PipelineResolver>();

        return services;
    }
}
