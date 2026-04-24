using YamlDotNet.Serialization;

namespace GitHooks.Pipeline.Parser.Yaml;

internal sealed class PipelineYamlDto
{
    [YamlMember(Alias = "parameters")]
    public List<ParameterYamlDto>? Parameters { get; init; }

    [YamlMember(Alias = "steps")]
    public List<StepYamlDto>? Steps { get; init; }
}
