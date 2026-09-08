using GitHooks.Compilation.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Infrastructure.Yaml.DependencyInjection;

/// <summary>
/// Registers the YAML parsing services required to produce an <see cref="IPipelineParser"/>
/// that turns YAML text into the <c>GitHooks.Domain</c> pipeline AST.
/// </summary>
public static class YamlPipelineCompilationRegistration
{
    /// <summary>
    /// Registers the full YAML parsing dependency chain (expressions, fields, parameters,
    /// steps, and the root pipeline parser), and exposes it as <see cref="IPipelineParser"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddYamlPipelineCompilation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // expressions
        _ = services.AddSingleton<VariableExpressionParser>();
        _ = services.AddSingleton<InterpolatedStringParser>();
        _ = services.AddSingleton<ExpressionParser>();

        // values
        _ = services.AddSingleton<FieldValueParser>();

        // fields
        _ = services.AddSingleton<FieldParser>();

        // parameters
        _ = services.AddSingleton<ParameterParser>();
        _ = services.AddSingleton<ParametersParser>();

        // steps
        _ = services.AddSingleton<StepParser>();
        _ = services.AddSingleton<StepsParser>();

        // pipeline
        _ = services.AddSingleton<PipelineParser>();

        _ = services.AddSingleton<IPipelineParser, YamlPipelineParser>();

        return services;
    }
}
