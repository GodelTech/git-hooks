using YamlDotNet.Serialization;

namespace GitHooks.Pipelines.Models;

public sealed class Step
{
    [YamlMember(Alias = "task")]
    public required string Task { get; init; }

    [YamlMember(Alias = "name")]
    public string? Name { get; init; }

    [YamlMember(Alias = "displayName")]
    public string? DisplayName { get; init; }

    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; init; } = true;

    [YamlMember(Alias = "continueOnError")]
    public bool ContinueOnError { get; init; } = false;

    [YamlMember(Alias = "inputs")]
    public Dictionary<string, object>? Inputs { get; init; }
}
