using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

internal sealed class DisplayNameFieldHandler : IStepFieldHandler
{
    public string Key => "displayName";

    public StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with
        {
            DisplayName = reader.Read<Scalar>().Value
        };
    }
}

