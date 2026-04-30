using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;
using GitHooks.Workflow.Infrastructure.Yaml.Steps;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.DependencyInjection;

public static class YamlParsingRegistration
{
    public static IServiceCollection AddYamlPipelineParsing(
        this IServiceCollection services)
    {
        _ = services.AddSingleton<ExpressionParser>();
        _ = services.AddSingleton<InterpolationParser>();

        // Register all IStepFieldHandler implementations
        var handlerType = typeof(IStepFieldHandler);

        var handlers = handlerType.Assembly
            .GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                handlerType.IsAssignableFrom(t)
            );

        foreach (var handler in handlers)
        {
            _ = services.AddSingleton(handlerType, handler);
        }

        _ = services.AddSingleton<StepParser>();
        _ = services.AddSingleton<StepsParser>();
        _ = services.AddSingleton<PipelineRootParser>();

        _ = services.AddSingleton<IPipelineParser, PipelineParser>();

        return services;
    }
}
