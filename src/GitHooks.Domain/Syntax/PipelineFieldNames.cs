namespace GitHooks.Domain.Syntax;

public static class PipelineFieldNames
{
    public const string Parameters = "parameters";
    public const string Steps = "steps";

    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>(
        [
            Parameters,
            Steps
        ],
        StringComparer.OrdinalIgnoreCase);
}
