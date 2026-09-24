using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Compilation.Binding;

internal static class ParameterSubstitutionRewriter
{
    /// <summary>
    /// Resolves an expression (e.g. a template step parameter override value) against the
    /// given enclosing scope, substituting any parameter variables/interpolations it contains,
    /// and returns the resulting literal string value.
    /// </summary>
    /// <param name="expression">The expression to resolve.</param>
    /// <param name="declarations">The parameter declarations of the enclosing scope.</param>
    /// <param name="values">The resolved parameter values of the enclosing scope.</param>
    /// <param name="diagnostics">The diagnostic bag to report unresolved references to.</param>
    /// <param name="value">The resolved literal string value, if resolution succeeded.</param>
    /// <returns><see langword="true"/> if the expression resolved to a literal string value; otherwise, <see langword="false"/>.</returns>
    public static bool TryResolveExpressionValue(
        ExpressionNode expression,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics,
        out string value)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (!IsFullyResolvable(expression, values))
        {
            diagnostics.Report(
                BuildUnresolvedParameterDiagnostic(expression, declarations, values));

            value = string.Empty;
            return false;
        }

        var rewritten = RewriteExpression(expression, declarations, values, diagnostics);

        return rewritten.TryGetStringValue(out value);
    }

    /// <summary>
    /// Rewrites parameter variable references within a set of steps using the given scope.
    /// Used to bind steps expanded from an included template against the template's own
    /// parameter scope (declarations plus resolved values/overrides).
    /// </summary>
    /// <param name="steps">The steps to rewrite.</param>
    /// <param name="declarations">The parameter declarations of the scope.</param>
    /// <param name="values">The resolved parameter values of the scope.</param>
    /// <param name="diagnostics">The diagnostic bag to report unresolved references to.</param>
    /// <returns>The rewritten steps.</returns>
    public static IReadOnlyList<StepNode> RewriteStepsForScope(
        IReadOnlyList<StepNode> steps,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(diagnostics);

        return RewriteSteps(steps, declarations, values, diagnostics);
    }

    public static PipelineNode Rewrite(
        PipelineNode root,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var parameters = RewriteParameters(root.Parameters, declarations, values, diagnostics);
        var steps = RewriteSteps(root.Steps, declarations, values, diagnostics);

        if (ReferenceEquals(parameters, root.Parameters) &&
            ReferenceEquals(steps, root.Steps))
        {
            return root;
        }

        return new PipelineNode
        {
            Parameters = parameters,
            Steps = steps,
            UnknownFields = root.UnknownFields,
            Span = root.Span
        };
    }

    private static IReadOnlyList<ParameterNode> RewriteParameters(
        IReadOnlyList<ParameterNode> parameters,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        List<ParameterNode>? rewritten = null;

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];

            var displayName = RewriteOptionalStringField(parameter.DisplayName, declarations, values, diagnostics);

            if (ReferenceEquals(displayName, parameter.DisplayName))
            {
                rewritten?.Add(parameter);
                continue;
            }

            rewritten ??= [.. parameters.Take(i)];
            rewritten.Add(new ParameterNode
            {
                Name = parameter.Name,
                DisplayName = displayName,
                Type = parameter.Type,
                DefaultValue = parameter.DefaultValue,
                Values = parameter.Values,
                UnknownFields = parameter.UnknownFields,
                Span = parameter.Span
            });
        }

        return rewritten ?? parameters;
    }

    private static IReadOnlyList<StepNode> RewriteSteps(
        IReadOnlyList<StepNode> steps,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        List<StepNode>? rewritten = null;

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var rewrittenStep = RewriteStep(step, declarations, values, diagnostics);

            if (ReferenceEquals(rewrittenStep, step))
            {
                rewritten?.Add(step);
                continue;
            }

            rewritten ??= [.. steps.Take(i)];
            rewritten.Add(rewrittenStep);
        }

        return rewritten ?? steps;
    }

    private static StepNode RewriteStep(
        StepNode step,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        return step switch
        {
            ScriptStepNode script => RewriteScriptStep(script, declarations, values, diagnostics),
            TemplateStepNode template => RewriteTemplateStep(template, declarations, values, diagnostics),
            _ => step
        };
    }

    private static ScriptStepNode RewriteScriptStep(
        ScriptStepNode step,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        var script = RewriteOptionalStringField(step.Script, declarations, values, diagnostics);
        var displayName = RewriteOptionalStringField(step.DisplayName, declarations, values, diagnostics);
        var condition = RewriteOptionalStringField(step.Condition, declarations, values, diagnostics);
        var timeoutInMinutes = RewriteOptionalStringField(step.TimeoutInMinutes, declarations, values, diagnostics);
        var workingDirectory = RewriteOptionalStringField(step.WorkingDirectory, declarations, values, diagnostics);
        var env = RewriteMapping(step.Env, declarations, values, diagnostics);

        if (ReferenceEquals(script, step.Script) &&
            ReferenceEquals(displayName, step.DisplayName) &&
            ReferenceEquals(condition, step.Condition) &&
            ReferenceEquals(timeoutInMinutes, step.TimeoutInMinutes) &&
            ReferenceEquals(workingDirectory, step.WorkingDirectory) &&
            ReferenceEquals(env, step.Env))
        {
            return step;
        }

        return new ScriptStepNode
        {
            Script = script!,
            DisplayName = displayName,
            Condition = condition,
            TimeoutInMinutes = timeoutInMinutes,
            WorkingDirectory = workingDirectory,
            Env = env,
            UnknownFields = step.UnknownFields,
            Span = step.Span
        };
    }

    private static TemplateStepNode RewriteTemplateStep(
        TemplateStepNode step,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        var template = RewriteOptionalStringField(step.Template, declarations, values, diagnostics);
        var parameters = RewriteMapping(step.Parameters, declarations, values, diagnostics);

        if (ReferenceEquals(template, step.Template) &&
            ReferenceEquals(parameters, step.Parameters))
        {
            return step;
        }

        return new TemplateStepNode
        {
            Template = template!,
            Parameters = parameters,
            UnknownFields = step.UnknownFields,
            Span = step.Span
        };
    }

    private static MappingFieldNode<StringKeyFieldNode<ExpressionNode>>? RewriteMapping(
        MappingFieldNode<StringKeyFieldNode<ExpressionNode>>? mapping,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        if (mapping is null)
        {
            return null;
        }

        List<StringKeyFieldNode<ExpressionNode>>? rewritten = null;

        for (var i = 0; i < mapping.Fields.Count; i++)
        {
            var field = mapping.Fields[i];
            var rewrittenField = RewriteOptionalStringField(field, declarations, values, diagnostics);

            if (ReferenceEquals(rewrittenField, field))
            {
                rewritten?.Add(field);
                continue;
            }

            rewritten ??= [.. mapping.Fields.Take(i)];
            rewritten.Add(rewrittenField!);
        }

        if (rewritten is null)
        {
            return mapping;
        }

        return new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
        {
            Key = mapping.Key,
            Fields = rewritten,
            Span = mapping.Span
        };
    }

    private static StringKeyFieldNode<ExpressionNode>? RewriteOptionalStringField(
        StringKeyFieldNode<ExpressionNode>? field,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        if (field is null)
        {
            return null;
        }

        var rewrittenValue = RewriteExpression(field.Value, declarations, values, diagnostics);

        if (ReferenceEquals(rewrittenValue, field.Value))
        {
            return field;
        }

        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = field.Key,
            Value = rewrittenValue,
            Span = field.Span
        };
    }

    private static ExpressionNode RewriteExpression(
        ExpressionNode expression,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        return expression switch
        {
            ParameterVariableExpressionNode parameterVariable
                => RewriteParameterVariable(parameterVariable, declarations, values, diagnostics),

            InterpolatedStringExpressionNode interpolated
                => RewriteInterpolatedString(interpolated, declarations, values, diagnostics),

            _ => expression
        };
    }

    private static StringLiteralExpressionNode RewriteParameterVariable(
        ParameterVariableExpressionNode node,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        if (values.TryGetValue(node.Name, out var value))
        {
            return new StringLiteralExpressionNode
            {
                Value = value,
                Span = node.Span
            };
        }

        if (declarations.ContainsParameter(node.Name))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterValueNotResolvable,
                    node.Span,
                    node.Name));
        }

        return new StringLiteralExpressionNode
        {
            Value = string.Empty,
            Span = node.Span
        };
    }

    private static ExpressionNode RewriteInterpolatedString(
        InterpolatedStringExpressionNode node,
        ParameterTable declarations,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        List<ExpressionNode>? rewritten = null;

        for (var i = 0; i < node.Parts.Count; i++)
        {
            var part = node.Parts[i];
            var rewrittenPart = RewriteExpression(part, declarations, values, diagnostics);

            if (ReferenceEquals(rewrittenPart, part))
            {
                rewritten?.Add(part);
                continue;
            }

            rewritten ??= [.. node.Parts.Take(i)];
            rewritten.Add(rewrittenPart);
        }

        if (rewritten is null)
        {
            return node;
        }

        if (rewritten.TrueForAll(static part => part is StringLiteralExpressionNode))
        {
            var combined = string.Concat(
                rewritten.ConvertAll(static part => ((StringLiteralExpressionNode)part).Value));

            return new StringLiteralExpressionNode
            {
                Value = combined,
                Span = node.Span
            };
        }

        return new InterpolatedStringExpressionNode
        {
            Parts = rewritten,
            Span = node.Span
        };
    }

    private static bool IsFullyResolvable(
        ExpressionNode expression,
        ParameterValueTable values)
    {
        return expression switch
        {
            ParameterVariableExpressionNode parameterVariable
                => values.ContainsValue(parameterVariable.Name),

            InterpolatedStringExpressionNode interpolated
                => interpolated.Parts.All(part => IsFullyResolvable(part, values)),

            _ => true
        };
    }

    private static Diagnostic BuildUnresolvedParameterDiagnostic(
        ExpressionNode expression,
        ParameterTable declarations,
        ParameterValueTable values)
    {
        var unresolved = FindFirstUnresolvedParameterVariable(expression, values)
            ?? throw new InvalidOperationException("Expected an unresolved parameter variable.");

        return declarations.ContainsParameter(unresolved.Name)
            ? Diagnostic.Create(
                DiagnosticDescriptors.ParameterValueNotResolvable,
                unresolved.Span,
                unresolved.Name)
            : Diagnostic.Create(
                DiagnosticDescriptors.ParameterVariableMustBeResolvable,
                unresolved.Span,
                unresolved.Name);
    }

    private static ParameterVariableExpressionNode? FindFirstUnresolvedParameterVariable(
        ExpressionNode expression,
        ParameterValueTable values)
    {
        switch (expression)
        {
            case ParameterVariableExpressionNode parameterVariable
                when !values.ContainsValue(parameterVariable.Name):
                return parameterVariable;

            case InterpolatedStringExpressionNode interpolated:
                foreach (var part in interpolated.Parts)
                {
                    var found = FindFirstUnresolvedParameterVariable(part, values);

                    if (found is not null)
                    {
                        return found;
                    }
                }

                return null;

            default:
                return null;
        }
    }
}
