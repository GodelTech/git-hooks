using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Builders;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

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

        // Register all IStepNodeBuilder implementations
        var builderType = typeof(IStepNodeBuilder);

        var builders = builderType.Assembly
            .GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                builderType.IsAssignableFrom(t)
            );

        foreach (var builder in builders)
        {
            _ = services.AddSingleton(builderType, builder);
        }

        // Register all IParameterFieldHandler implementations
        var parameterHandlerType = typeof(IParameterFieldHandler);

        var parameterHandlers = parameterHandlerType.Assembly
            .GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                parameterHandlerType.IsAssignableFrom(t)
            );

        foreach (var parameterHandler in parameterHandlers)
        {
            _ = services.AddSingleton(parameterHandlerType, parameterHandler);
        }

        _ = services.AddSingleton<IParameterNodeBuilder, ParameterNodeBuilder>();

        _ = services.AddSingleton<StepParser>();
        _ = services.AddSingleton<StepsParser>();
        _ = services.AddSingleton<ParameterParser>();
        _ = services.AddSingleton<PipelineParametersParser>();
        _ = services.AddSingleton<PipelineRootParser>();

        _ = services.AddSingleton<IPipelineParser, PipelineParser>();

        return services;
    }
}
