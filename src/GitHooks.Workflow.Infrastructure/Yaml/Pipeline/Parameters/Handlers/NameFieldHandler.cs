using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

internal sealed class NameFieldHandler : IParameterFieldHandler
{
    public string Key => "name";

    public ParameterFields Apply(YamlReader reader, ParameterFields fields)
    {
        return fields with
        {
            Name = reader.Read<Scalar>().Value
        };
    }
}
