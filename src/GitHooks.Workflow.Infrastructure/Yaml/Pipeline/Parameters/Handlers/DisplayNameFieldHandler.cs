using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

internal sealed class DisplayNameFieldHandler : IParameterFieldHandler
{
    public string Key => "displayName";

    public ParameterFields Apply(YamlReader reader, ParameterFields fields)
    {
        return fields with
        {
            DisplayName = reader.Read<Scalar>().Value
        };
    }
}
