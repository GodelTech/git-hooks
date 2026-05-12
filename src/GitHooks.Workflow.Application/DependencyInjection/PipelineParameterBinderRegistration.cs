using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Steps;
using GitHooks.Workflow.Application.Binding.UnknownNodes;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.DependencyInjection;

public static class PipelineParameterBinderRegistration
{
    public static IServiceCollection AddPipelineParameterBinding(this IServiceCollection services)
    {
        _ = services.AddSingleton<UnknownNodeParameterBinder>();
        _ = services.AddSingleton<StepParameterBinder>();
        _ = services.AddSingleton<IPipelineParameterBinder, PipelineParameterBinder>();

        return services;
    }
}
