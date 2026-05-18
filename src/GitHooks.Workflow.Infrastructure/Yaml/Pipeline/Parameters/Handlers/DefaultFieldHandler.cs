using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

internal sealed class DefaultFieldHandler
    : IParameterFieldHandler
{
    public string Key => "default";

    public ParameterFields Apply(YamlReader reader, ParameterFields fields)
    {
        // Default may be any scalar: string, boolean literal, or number.
        return fields with
        {
            DefaultValue = reader.Read<Scalar>().Value
        };
    }
}
