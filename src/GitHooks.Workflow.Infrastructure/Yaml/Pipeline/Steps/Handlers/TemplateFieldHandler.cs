using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

internal sealed class TemplateFieldHandler : IStepFieldHandler
{
    public string Key => "template";

    public StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with
        {
            Template = reader.Read<Scalar>().Value
        };
    }
}

