using YamlDotNet.Serialization;

namespace GitHooks.Pipelines.Models;

public sealed class Pipeline
{
    [YamlMember(Alias = "steps")]
    public List<Step> Steps { get; init; } = [];
}
