using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Infrastructure.Expansion;
using GitHooks.Workflow.Infrastructure.IO;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.DependencyInjection;

public static class PipelineTemplateExpansionRegistration
{
    public static IServiceCollection AddPipelineTemplateExpansion(this IServiceCollection services)
    {
        _ = services.AddPipelineCompilation();
        _ = services.AddPipelineTemplateExpansionOrchestration();

        _ = services.AddSingleton<IPipelineContentSourceReader, LocalPipelineContentReader>();
        _ = services.AddSingleton<IPipelineContentSourceReader, RemoteRepositoryPipelineContentReader>();
        _ = services.AddSingleton<IPipelineContentReader, CompositePipelineContentReader>();

        _ = services.AddSingleton<IPipelineTemplateSourceResolverStrategy, LocalPipelineTemplateSourceResolver>();
        _ = services.AddSingleton<IPipelineTemplateSourceResolverStrategy, RemoteRepositoryPipelineTemplateSourceResolver>();
        _ = services.AddSingleton<IPipelineTemplateSourceResolver, CompositePipelineTemplateSourceResolver>();

        _ = services.AddSingleton<IPipelineTemplateExpander, PipelineTemplateExpander>();

        return services;
    }
}
