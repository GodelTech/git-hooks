using System.Text.RegularExpressions;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Validation.Rules.Pipeline;

internal sealed partial class ParameterVariableMustBeResolvableRule
    : IValidationRule<PipelineNode>
{
    public void Validate(
        PipelineNode node,
        ValidationContext context)
    {
        var definedParameterNames = new HashSet<string>();

        foreach (var parameter in node.Parameters)
        {
            if (parameter.Name.Value is StringLiteralExpressionNode nameLiteral &&
                !string.IsNullOrWhiteSpace(nameLiteral.Value))
            {
                _ = definedParameterNames.Add(nameLiteral.Value);
            }
        }

        // Check steps for variable references
        foreach (var step in node.Steps)
        {
            if (step is ScriptStepNode scriptStep)
            {
                CheckExpressionForVariables(scriptStep.Script.Value, definedParameterNames, context);

                if (scriptStep.DisplayName is not null)
                {
                    CheckExpressionForVariables(scriptStep.DisplayName.Value, definedParameterNames, context);
                }

                if (scriptStep.Condition is not null)
                {
                    CheckExpressionForVariables(scriptStep.Condition.Value, definedParameterNames, context);
                }

                if (scriptStep.WorkingDirectory is not null)
                {
                    CheckExpressionForVariables(scriptStep.WorkingDirectory.Value, definedParameterNames, context);
                }

                if (scriptStep.TimeoutInMinutes is not null)
                {
                    CheckExpressionForVariables(scriptStep.TimeoutInMinutes.Value, definedParameterNames, context);
                }

                if (scriptStep.Env is not null)
                {
                    foreach (var envVar in scriptStep.Env.Fields)
                    {
                        CheckExpressionForVariables(envVar.Value, definedParameterNames, context);
                    }
                }
            }
            else if (step is TemplateStepNode templateStep)
            {
                CheckExpressionForVariables(templateStep.Template.Value, definedParameterNames, context);

                if (templateStep.Parameters is not null)
                {
                    foreach (var param in templateStep.Parameters.Fields)
                    {
                        CheckExpressionForVariables(param.Value, definedParameterNames, context);
                    }
                }
            }
        }
    }

    [GeneratedRegex(@"\$\{\{\s*parameters\.([a-zA-Z_][a-zA-Z0-9_]*)\s*\}\}")]
    private static partial Regex ExtractParameterNamePattern();

    [GeneratedRegex(@"^parameters\.([a-zA-Z_][a-zA-Z0-9_]*)$")]
    private static partial Regex ParameterPathPattern();

    private static void CheckExpressionForVariables(
        ExpressionNode expression,
        HashSet<string> definedParameterNames,
        ValidationContext context)
    {
        // Handle VariableExpressionNode separately - it has clean path format (no braces)
        if (expression is VariableExpressionNode variable)
        {
            // Variable path is already clean: "parameters.name"
            var match = ParameterPathPattern().Match(variable.Path);
            if (match.Success)
            {
                var paramName = match.Groups[1].Value;
                if (!definedParameterNames.Contains(paramName))
                {
                    context.Report(
                        DiagnosticDescriptors.ParameterVariableMustBeResolvable,
                        expression.Span,
                        paramName);
                }
            }

            return;
        }

        var textToCheck = expression switch
        {
            StringLiteralExpressionNode literal => literal.Value,
            InterpolatedStringExpressionNode interpolated =>
                string.Concat(interpolated.Parts.Select(p => p switch
                {
                    StringLiteralExpressionNode literal => literal.Value,
                    _ => string.Empty,
                })),
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(textToCheck))
        {
            return;
        }

        var matches = ExtractParameterNamePattern().Matches(textToCheck);
        foreach (var match in matches)
        {
            var paramName = ((Match)match).Groups[1].Value;
            if (!definedParameterNames.Contains(paramName))
            {
                context.Report(
                    DiagnosticDescriptors.ParameterVariableMustBeResolvable,
                    expression.Span,
                    paramName);
            }
        }
    }
}
