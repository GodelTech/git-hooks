using GitHooks.Domain.Syntax;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal static class StepValidation
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
        var violations = new List<string>();

        if (step.Script is not null && !allowedFields.Contains(StepFieldNames.Script))
        {
            violations.Add(StepFieldNames.Script);
        }

        if (step.Template is not null && !allowedFields.Contains(StepFieldNames.Template))
        {
            violations.Add(StepFieldNames.Template);
        }

        if (step.DisplayName is not null && !allowedFields.Contains(StepFieldNames.DisplayName))
        {
            violations.Add(StepFieldNames.DisplayName);
        }

        if (step.Condition is not null && !allowedFields.Contains(StepFieldNames.Condition))
        {
            violations.Add(StepFieldNames.Condition);
        }

        if (step.TimeoutInMinutes is not null && !allowedFields.Contains(StepFieldNames.TimeoutInMinutes))
        {
            violations.Add(StepFieldNames.TimeoutInMinutes);
        }

        if (step.WorkingDirectory is not null && !allowedFields.Contains(StepFieldNames.WorkingDirectory))
        {
            violations.Add(StepFieldNames.WorkingDirectory);
        }

        if (step.Env.Count is > 0 && !allowedFields.Contains(StepFieldNames.Env))
        {
            violations.Add(StepFieldNames.Env);
        }

        if (step.Parameters.Count is > 0 && !allowedFields.Contains(StepFieldNames.Parameters))
        {
            violations.Add(StepFieldNames.Parameters);
        }

        if (violations.Count > 0)
        {
            throw cursor.CreateParsingException(
                $"{stepType} step contains invalid field(s): {string.Join(", ", violations)}");
        }
    }
}
