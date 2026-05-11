namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

internal interface IStepFieldHandler
{
    public string Key { get; }

    public StepFields Apply(YamlReader reader, StepFields fields);
}

