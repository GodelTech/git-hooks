using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

namespace GitHooks.Infrastructure.Yaml.Tests;

internal static class TestParserFactory
{
    public static YamlPipelineParser Create()
    {
        // unknown
        var unknownNodeParser =
            new UnknownNodeParser();

        // parameters
        var parameterParser =
            new ParameterParser(
                unknownNodeParser);

        var parametersParser =
            new ParametersParser(
                parameterParser);

        // steps
        var stepsParser =
            new StepsParser();

        var pipelineParser =
            new PipelineParser(
                parametersParser,
                stepsParser,
                unknownNodeParser);

        return new YamlPipelineParser(
            pipelineParser);
    }
}
