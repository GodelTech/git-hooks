using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Infrastructure.Expansion;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.DependencyInjection;

public static class PipelineTemplateExpansionRegistration
{
    public static IServiceCollection AddPipelineTemplateExpansion(this IServiceCollection services)
    {
        _ = services.AddPipelineCompilation();
        _ = services.AddPipelineTemplateExpansionOrchestration();
        _ = services.AddSingleton<ITemplatePathResolver, TemplatePathResolver>();
        _ = services.AddSingleton<IPipelineTemplateExpander, PipelineTemplateExpander>();

        return services;
    }
}
