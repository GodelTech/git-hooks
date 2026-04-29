using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Infrastructure.Yaml.Sections;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal static class PipelineParserFactory
{
    public static IPipelineParser Create(IYamlParserFactory yamlFactory)
    {
        // Step parsing
        var stepParser = new StepParser();
        var stepsParser = new StepsParser(stepParser);

        // Root parser
        var rootParser = new PipelineRootParser(stepsParser);

        // Public entry point
        return new PipelineParser(yamlFactory, rootParser);
    }
}
