using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Domain.Syntax;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal static class StepFieldValidation
{
    public static void ValidateScriptStep(
        StepFields step,
        SourceSpan span,
        ParsingContext context)
    {
        ValidateAllowedFields(
            "script",
            step,
            span,
            context,
            StepFieldNames.ScriptStepFields);
    }

    public static void ValidateTemplateStep(
        StepFields step,
        SourceSpan span,
        ParsingContext context)
    {
        ValidateAllowedFields(
            "template",
            step,
            span,
            context,
            StepFieldNames.TemplateStepFields);
    }

    private static void ValidateAllowedFields(
        string stepType,
        StepFields step,
        SourceSpan span,
        ParsingContext context,
        IReadOnlySet<string> allowedFields)
    {
        var violations = step
            .GetPresentFields()
            .Where(field => !allowedFields.Contains(field))
            .ToArray();

        foreach (var field in violations)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidStepField,
                    span,
                    field,
                    stepType));
        }
    }
}
