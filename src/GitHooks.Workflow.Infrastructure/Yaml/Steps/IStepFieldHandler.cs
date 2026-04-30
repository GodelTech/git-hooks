namespace GitHooks.Workflow.Infrastructure.Yaml.Steps;

internal interface IStepFieldHandler
{
    public string Key { get; }

    public StepFields Apply(YamlReader reader, StepFields fields);
}
