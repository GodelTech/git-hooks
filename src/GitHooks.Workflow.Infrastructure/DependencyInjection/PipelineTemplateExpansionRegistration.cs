using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Source;
using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Infrastructure.Expansion.Source;
using GitHooks.Workflow.Infrastructure.IO;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.DependencyInjection;

public static class PipelineTemplateExpansionRegistration
{
    public static IServiceCollection AddPipelineTemplateExpansion(this IServiceCollection services)
    {
        _ = services.AddPipelineCompilation();
        _ = services.AddPipelineTemplateExpansionOrchestration();

        _ = services.AddSingleton<IPipelineSourceContentReader, LocalPipelineSourceContentReader>();
        _ = services.AddSingleton<IPipelineSourceContentReader, RemoteRepositoryPipelineSourceContentReader>();
        _ = services.AddSingleton<IPipelineContentReader, PipelineContentReader>();

        _ = services.AddSingleton<IPipelineTemplateSourceResolver, LocalPipelineTemplateSourceResolver>();
        _ = services.AddSingleton<IPipelineTemplateSourceResolver, RemoteRepositoryPipelineTemplateSourceResolver>();
        _ = services.AddSingleton<PipelineTemplateResolver>();

        _ = services.AddSingleton<IPipelineTemplateExpander, PipelineTemplateExpander>();

        return services;
    }
}
