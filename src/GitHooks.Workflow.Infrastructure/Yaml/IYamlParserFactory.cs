namespace GitHooks.Workflow.Infrastructure.Yaml;

internal interface IYamlParserFactory
{
    public YamlReader Create(string yaml, string sourceName);
}
