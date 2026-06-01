namespace GitHooks.Domain.Syntax;

public static class ParameterFieldNames
{
    public const string Name = "name";
    public const string DisplayName = "displayName";
    public const string Type = "type";
    public const string DefaultValue = "default";
    public const string Values = "values";

    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>(
        [
            Name,
            DisplayName,
            Type,
            DefaultValue,
            Values
        ],
        StringComparer.OrdinalIgnoreCase);
}
