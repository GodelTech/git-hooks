using GitHooks.Domain.Syntax;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal static class StepFieldValidation
{
    public static void ValidateScriptStep(
        StepFields step,
        YamlParserCursor cursor)
    {
        ValidateAllowedFields(
            "Script",
            step,
            cursor,
            StepFieldNames.ScriptStepFields);
    }

    public static void ValidateTemplateStep(
        StepFields step,
        YamlParserCursor cursor)
    {
        ValidateAllowedFields(
            "Template",
            step,
            cursor,
            StepFieldNames.TemplateStepFields);
    }

    private static void ValidateAllowedFields(
        string stepType,
        StepFields step,
        YamlParserCursor cursor,
        IReadOnlySet<string> allowedFields)
    {
        var violations = step
            .GetPresentFields()
            .Where(field => !allowedFields.Contains(field))
            .ToArray();

        if (violations.Length > 0)
        {
            throw cursor.CreateException(
                $"{stepType} step contains invalid field(s): {string.Join(", ", violations)}");
        }
    }
}
