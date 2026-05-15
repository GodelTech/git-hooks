using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Expressions;
using GitHooks.Workflow.Application.Binding.Pipeline;
using GitHooks.Workflow.Application.Binding.Pipeline.Steps;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.DependencyInjection;

public static class PipelineParameterBinderRegistration
{
    public static IServiceCollection AddPipelineParameterBinding(this IServiceCollection services)
    {
        // Register core binders
        _ = services.AddSingleton<InterpolationParameterBinder>();
        _ = services.AddSingleton<StringBinder>();
        _ = services.AddSingleton<UnknownNodeParameterBinder>();

        // Register all IStepNodeParameterBinder implementations
        var binderType = typeof(IStepNodeParameterBinder);

        var binders = binderType.Assembly
            .GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                binderType.IsAssignableFrom(t)
            );

        foreach (var binder in binders)
        {
            _ = services.AddSingleton(binderType, binder);
        }

        // Register the step binder coordinator
        _ = services.AddSingleton<StepParameterBinder>();
        _ = services.AddSingleton<StepsParameterBinder>();
        _ = services.AddSingleton<PipelineRootParameterBinder>();

        // Register the pipeline binder facade
        _ = services.AddSingleton<IPipelineParameterBinder, PipelineParameterBinder>();

        return services;
    }
}
