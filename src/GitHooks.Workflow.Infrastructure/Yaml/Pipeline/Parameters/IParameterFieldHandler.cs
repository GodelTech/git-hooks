namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

internal interface IParameterFieldHandler
{
    public string Key { get; }

    public ParameterFields Apply(YamlReader reader, ParameterFields fields);
}
