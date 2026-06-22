using GitHooks.Diagnostics;
using GitHooks.Domain.Syntax;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal static class StepFieldValidation
{
    public static void ValidateScriptStep(
        StepFields step,
        ParsingContext context)
    {
        ValidateAllowedFields(
            "script",
            step,
            context,
            StepFieldNames.ScriptStepFields);
    }

    public static void ValidateTemplateStep(
        StepFields step,
        ParsingContext context)
    {
        ValidateAllowedFields(
            "template",
            step,
            context,
            StepFieldNames.TemplateStepFields);
    }

    private static void ValidateAllowedFields(
        string stepType,
        StepFields step,
        ParsingContext context,
        IReadOnlySet<string> allowedFields)
    {
        var violations = step
            .GetFields()
            .Where(field => !allowedFields.Contains(field.Name))
            .ToArray();

        foreach (var (name, field) in violations)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidStepField,
                    field.Span,
                    name,
                    stepType));
        }
    }
}
