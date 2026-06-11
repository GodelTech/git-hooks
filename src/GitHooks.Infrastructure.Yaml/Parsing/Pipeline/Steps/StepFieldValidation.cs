using GitHooks.Domain.Syntax;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal static class StepFieldValidation
{
    public static void ValidateScriptStep(
        StepFields step,
        ParsingContext context)
    {
        ValidateAllowedFields(
            "Script",
            step,
            context,
            StepFieldNames.ScriptStepFields);
    }

    public static void ValidateTemplateStep(
        StepFields step,
        ParsingContext context)
    {
        ValidateAllowedFields(
            "Template",
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
            .GetPresentFields()
            .Where(field => !allowedFields.Contains(field))
            .ToArray();

        if (violations.Length > 0)
        {
            throw context.Cursor.CreateException(
                $"{stepType} step contains invalid field(s): {string.Join(", ", violations)}");
        }
    }
}
