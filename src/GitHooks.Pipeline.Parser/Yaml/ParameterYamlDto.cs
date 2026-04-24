using YamlDotNet.Serialization;

namespace GitHooks.Pipeline.Parser.Yaml;

internal sealed class ParameterYamlDto
{
    [YamlMember(Alias = "name")]
    public string? Name { get; init; }

    [YamlMember(Alias = "displayName")]
    public string? DisplayName { get; init; }

    [YamlMember(Alias = "type")]
    public string? Type { get; init; }

    [YamlMember(Alias = "default")]
    public object? Default { get; init; }

    [YamlMember(Alias = "values")]
    public List<object?>? Values { get; init; }
}
