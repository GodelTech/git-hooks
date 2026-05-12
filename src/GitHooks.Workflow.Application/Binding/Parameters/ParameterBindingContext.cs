using System.Globalization;

using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Parameters;

internal sealed class ParameterBindingContext(
    IReadOnlyDictionary<string, ParameterNode> declarations,
    IReadOnlyDictionary<string, string> resolvedValues)
{
    private readonly IReadOnlyDictionary<string, ParameterNode> _declarations = declarations;
    private readonly IReadOnlyDictionary<string, string> _resolvedValues = resolvedValues;

    public static ParameterBindingContext Create(
        IReadOnlyList<ParameterNode> parameters,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
    {
        var declarations = new Dictionary<string, ParameterNode>(StringComparer.Ordinal);
        var resolvedValues = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var parameter in parameters)
        {
            if (!declarations.TryAdd(parameter.Name, parameter))
            {
                throw new PipelineParameterBindingException(
                    $"Parameter '{parameter.Name}' is declared more than once.",
                    parameter.Span
                );
            }

            if (parameter.Default is null)
            {
                continue;
            }

            ValidateParameterValue(parameter, parameter.Default, parameter.Span);
            resolvedValues[parameter.Name] = parameter.Default;
        }

        if (parameterOverrides is not null)
        {
            var commandLineSpan = SourceSpan.Unknown(new SourceRef("command-line"));

            foreach (var parameterOverride in parameterOverrides)
            {
                if (!declarations.TryGetValue(parameterOverride.Key, out var declaration))
                {
                    throw new PipelineParameterBindingException(
                        $"Parameter '{parameterOverride.Key}' is not defined.",
                        commandLineSpan
                    );
                }

                ValidateParameterValue(declaration, parameterOverride.Value, declaration.Span);
                resolvedValues[parameterOverride.Key] = parameterOverride.Value;
            }
        }

        return new ParameterBindingContext(declarations, resolvedValues);
    }

    public string GetResolvedValue(string parameterName, SourceSpan span)
    {
        if (_resolvedValues.TryGetValue(parameterName, out var value))
        {
            return value;
        }

        if (_declarations.ContainsKey(parameterName))
        {
            throw new PipelineParameterBindingException(
                $"Parameter '{parameterName}' does not have a value.",
                span
            );
        }

        throw new PipelineParameterBindingException(
            $"Parameter '{parameterName}' is not defined.",
            span
        );
    }

    private static void ValidateParameterValue(ParameterNode parameter, string value, SourceSpan span)
    {
        if (parameter.Type is ParameterType.Boolean && !bool.TryParse(value, out _))
        {
            throw new PipelineParameterBindingException(
                $"Parameter '{parameter.Name}' expects a boolean value but received '{value}'.",
                span
            );
        }

        if (parameter.Type is ParameterType.Number &&
            !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            throw new PipelineParameterBindingException(
                $"Parameter '{parameter.Name}' expects a number value but received '{value}'.",
                span
            );
        }

        if (parameter.Values.Count > 0 && !parameter.Values.Contains(value, StringComparer.Ordinal))
        {
            throw new PipelineParameterBindingException(
                $"Parameter '{parameter.Name}' value '{value}' is not in the allowed values list.",
                span
            );
        }
    }
}
