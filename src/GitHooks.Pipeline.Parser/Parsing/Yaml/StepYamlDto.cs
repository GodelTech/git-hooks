using YamlDotNet.Serialization;

namespace GitHooks.Pipeline.Parser.Parsing.Yaml;

internal sealed class StepYamlDto
{
    [YamlMember(Alias = "task")]
    public string? Task { get; init; }

    [YamlMember(Alias = "script")]
    public string? Script { get; init; }

    [YamlMember(Alias = "pwsh")]
    public string? Pwsh { get; init; }

    [YamlMember(Alias = "name")]
    public string? Name { get; init; }

    [YamlMember(Alias = "displayName")]
    public string? DisplayName { get; init; }

    [YamlMember(Alias = "enabled")]
    public object? Enabled { get; init; }

    [YamlMember(Alias = "continueOnError")]
    public object? ContinueOnError { get; init; }

    [YamlMember(Alias = "inputs")]
    public Dictionary<string, object?>? Inputs { get; init; }
}
