using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Steps.Handlers;

internal sealed class TimeoutFieldHandler : IStepFieldHandler
{
    public string Key => "timeoutInMinutes";

    public StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with
        {
            TimeoutInMinutes = int.Parse(reader.Read<Scalar>().Value)
        };
    }
}
