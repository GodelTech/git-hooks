using GitHooks.Workflow.Application.Expansion;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.DependencyInjection;

public static class PipelineTemplateExpansionOrchestrationRegistration
{
    public static IServiceCollection AddPipelineTemplateExpansionOrchestration(this IServiceCollection services)
    {
        _ = services.AddSingleton<ITemplateExpansionOrchestrator, TemplateExpansionOrchestrator>();

        return services;
    }
}
