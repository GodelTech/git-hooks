namespace GitHooks.Domain.Syntax;

public static class PipelineFieldNames
{
    public const string Parameters = "parameters";
    public const string Steps = "steps";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(
        [
            Parameters,
            Steps
        ],
        StringComparer.OrdinalIgnoreCase);
}
