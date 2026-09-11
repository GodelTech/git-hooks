using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Compilation.Binding;

internal static class ParameterSubstitutionRewriter
{
    public static PipelineNode Rewrite(
        PipelineNode root,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var parameters = RewriteParameters(root.Parameters, values, diagnostics);
        var steps = RewriteSteps(root.Steps, values, diagnostics);

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
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        List<ParameterNode>? rewritten = null;

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];

            var displayName = RewriteOptionalStringField(parameter.DisplayName, values, diagnostics);

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
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        List<StepNode>? rewritten = null;

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var rewrittenStep = RewriteStep(step, values, diagnostics);

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
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        return step switch
        {
            ScriptStepNode script => RewriteScriptStep(script, values, diagnostics),
            TemplateStepNode template => RewriteTemplateStep(template, values, diagnostics),
            _ => step
        };
    }

    private static ScriptStepNode RewriteScriptStep(
        ScriptStepNode step,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        var script = RewriteOptionalStringField(step.Script, values, diagnostics);
        var displayName = RewriteOptionalStringField(step.DisplayName, values, diagnostics);
        var condition = RewriteOptionalStringField(step.Condition, values, diagnostics);
        var timeoutInMinutes = RewriteOptionalStringField(step.TimeoutInMinutes, values, diagnostics);
        var workingDirectory = RewriteOptionalStringField(step.WorkingDirectory, values, diagnostics);
        var env = RewriteMapping(step.Env, values, diagnostics);

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
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        var template = RewriteOptionalStringField(step.Template, values, diagnostics);
        var parameters = RewriteMapping(step.Parameters, values, diagnostics);

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
            var rewrittenField = RewriteOptionalStringField(field, values, diagnostics);

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
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        if (field is null)
        {
            return null;
        }

        var rewrittenValue = RewriteExpression(field.Value, values, diagnostics);

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
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        return expression switch
        {
            ParameterVariableExpressionNode parameterVariable
                => RewriteParameterVariable(parameterVariable, values, diagnostics),

            InterpolatedStringExpressionNode interpolated
                => RewriteInterpolatedString(interpolated, values, diagnostics),

            _ => expression
        };
    }

    private static StringLiteralExpressionNode RewriteParameterVariable(
        ParameterVariableExpressionNode node,
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

        diagnostics.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.ParameterValueNotResolvable,
                node.Span,
                node.Name));

        return new StringLiteralExpressionNode
        {
            Value = string.Empty,
            Span = node.Span
        };
    }

    private static ExpressionNode RewriteInterpolatedString(
        InterpolatedStringExpressionNode node,
        ParameterValueTable values,
        DiagnosticBag diagnostics)
    {
        List<ExpressionNode>? rewritten = null;

        for (var i = 0; i < node.Parts.Count; i++)
        {
            var part = node.Parts[i];
            var rewrittenPart = RewriteExpression(part, values, diagnostics);

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
}
