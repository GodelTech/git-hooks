namespace GitHooks.Domain.Syntax;

public static class StepFieldNames
{
    public const string Script = "script";
    public const string Template = "template";
    public const string DisplayName = "displayName";
    public const string Condition = "condition";
    public const string TimeoutInMinutes = "timeoutInMinutes";
    public const string WorkingDirectory = "workingDirectory";
    public const string Env = "env";
    public const string Parameters = "parameters";

    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>(
        [
            Script,
            Template,
            DisplayName,
            Condition,
            TimeoutInMinutes,
            WorkingDirectory,
            Env,
            Parameters
        ],
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> ScriptStepFields { get; } =
        new HashSet<string>(
        [
            Script,
            DisplayName,
            Condition,
            TimeoutInMinutes,
            WorkingDirectory,
            Env
        ],
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> TemplateStepFields { get; } =
        new HashSet<string>(
        [
            Template,
            Parameters
        ],
        StringComparer.OrdinalIgnoreCase);
}
