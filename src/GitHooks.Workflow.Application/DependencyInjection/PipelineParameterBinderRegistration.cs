using GitHooks.Workflow.Application.Binding;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.DependencyInjection;

public static class PipelineParameterBinderRegistration
{
    public static IServiceCollection AddPipelineParameterBinding(this IServiceCollection services)
    {
        _ = services.AddSingleton<IPipelineParameterBinder, PipelineParameterBinder>();

        return services;
    }
}

