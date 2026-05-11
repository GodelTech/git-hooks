using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding;

/// <summary>
/// Applies Azure DevOps-style <c>${{ parameters.name }}</c> substitutions to a parsed pipeline AST.
/// </summary>
public sealed partial class PipelineParameterBinder : IPipelineParameterBinder
{
    [GeneratedRegex("\\$\\{\\{(?<expression>.*?)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex InterpolationPattern();

    /// <inheritdoc/>
    public PipelineNode Bind(PipelineNode pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var context = ParameterBindingContext.Create(pipeline.Parameters);

        return pipeline with
        {
            Steps = [.. pipeline.Steps.Select(step => BindStep(step, context))],
            UnknownFields = BindUnknownFields(pipeline.UnknownFields, context)
        };
    }

    private static StepNode BindStep(StepNode step, ParameterBindingContext context)
    {
        var boundStep = step switch
        {
            ScriptStepNode scriptStep => BindScriptStep(scriptStep, context),
            TemplateStepNode templateStep => BindTemplateStep(templateStep, context),
            _ => throw new PipelineParameterBindingException(
                $"Unsupported step node type '{step.GetType().Name}'.",
                step.Span
            )
        };

        return boundStep with
        {
            DisplayName = boundStep.DisplayName is null
                ? null
                : BindScalar(boundStep.DisplayName, boundStep.Span, context),
            WorkingDirectory = boundStep.WorkingDirectory is null
                ? null
                : BindInterpolatedString(boundStep.WorkingDirectory, boundStep.Span, context),
            Env = BindInterpolatedStringMap(boundStep.Env, context),
            UnknownFields = BindUnknownFields(boundStep.UnknownFields, context)
        };
    }

    private static StepNode BindScriptStep(ScriptStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Script = BindInterpolatedString(step.Script, step.Span, context)
        };
    }

    private static StepNode BindTemplateStep(TemplateStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Parameters = BindInterpolatedStringMap(step.Parameters, context),
            Template = BindScalar(step.Template, step.Span, context),
        };
    }

    private static Dictionary<string, InterpolatedStringNode> BindInterpolatedStringMap(
        IReadOnlyDictionary<string, InterpolatedStringNode> values,
        ParameterBindingContext context)
    {
        return values.ToDictionary(
            pair => pair.Key,
            pair => BindInterpolatedString(pair.Value, pair.ValueSpan(), context),
            StringComparer.Ordinal
        );
    }

    private static InterpolatedStringNode BindInterpolatedString(
        InterpolatedStringNode node,
        SourceSpan span,
        ParameterBindingContext context)
    {
        return node with
        {
            Value = BindScalar(node.Value, span, context)
        };
    }

    private static UnknownFieldNode[] BindUnknownFields(
        IReadOnlyList<UnknownFieldNode> unknownFields,
        ParameterBindingContext context)
    {
        return
        [
            .. unknownFields.Select(
                field => field with
                {
                    Key = BindUnknownNode(field.Key, context),
                    Value = BindUnknownNode(field.Value, context)
                }
            )
        ];
    }

    private static UnknownNode BindUnknownNode(UnknownNode node, ParameterBindingContext context)
    {
        return node switch
        {
            UnknownScalarNode scalarNode => scalarNode with
            {
                Value = BindScalar(scalarNode.Value, scalarNode.Span, context)
            },
            UnknownMappingNode mappingNode => mappingNode with
            {
                Entries =
                [
                    .. mappingNode.Entries.Select(
                        entry => entry with
                        {
                            Key = BindUnknownNode(entry.Key, context),
                            Value = BindUnknownNode(entry.Value, context)
                        }
                    )
                ]
            },
            UnknownSequenceNode sequenceNode => sequenceNode with
            {
                Items = [.. sequenceNode.Items.Select(item => BindUnknownNode(item, context))]
            },
            UnknownReferenceNode referenceNode => referenceNode,
            _ => throw new PipelineParameterBindingException(
                $"Unsupported unknown node type '{node.GetType().Name}'.",
                node.Span
            )
        };
    }

    private static string BindScalar(string value, SourceSpan span, ParameterBindingContext context)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var matches = InterpolationPattern().Matches(value);

        if (matches.Count == 0)
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);
        var currentIndex = 0;

        foreach (Match match in matches)
        {
            _ = builder.Append(value, currentIndex, match.Index - currentIndex);

            var expression = match.Groups["expression"].Value.Trim();
            _ = builder.Append(ResolveExpression(expression, span, context));

            currentIndex = match.Index + match.Length;
        }

        _ = builder.Append(value, currentIndex, value.Length - currentIndex);
        return builder.ToString();
    }

    private static string ResolveExpression(string expression, SourceSpan span, ParameterBindingContext context)
    {
        const string parameterPrefix = "parameters.";

        if (!expression.StartsWith(parameterPrefix, StringComparison.Ordinal))
        {
            throw new PipelineParameterBindingException(
                $"Unsupported expression '{expression}'. Only 'parameters.<name>' is currently supported.",
                span
            );
        }

        var parameterName = expression[parameterPrefix.Length..].Trim();

        _ = string.IsNullOrWhiteSpace(parameterName)
            ? throw new PipelineParameterBindingException(
                "Parameter expression must specify a parameter name.",
                span
            )
            : parameterName;

        return context.GetResolvedValue(parameterName, span);
    }

    private sealed class ParameterBindingContext(
        IReadOnlyDictionary<string, ParameterNode> declarations,
        IReadOnlyDictionary<string, string> resolvedValues)
    {
        public static ParameterBindingContext Create(IReadOnlyList<ParameterNode> parameters)
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

            return new ParameterBindingContext(declarations, resolvedValues);
        }

        public string GetResolvedValue(string parameterName, SourceSpan span)
        {
            if (resolvedValues.TryGetValue(parameterName, out var value))
            {
                return value;
            }

            if (declarations.ContainsKey(parameterName))
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
}

file static class InterpolatedStringNodeExtensions
{
    public static SourceSpan ValueSpan(this KeyValuePair<string, InterpolatedStringNode> pair)
    {
        return SourceSpan.Unknown(new SourceRef(pair.Key));
    }
}
