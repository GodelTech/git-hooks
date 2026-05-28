using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
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

        // expressions
        var expressionParser =
            new ExpressionParser();

        // parameters
        var parameterParser =
            new ParameterParser(
                expressionParser,
                unknownNodeParser);

        var parametersParser =
            new ParametersParser(
                parameterParser);

        // steps
        var stepParser =
            new StepParser();

        var stepsParser =
            new StepsParser(
                stepParser);

        var pipelineParser =
            new PipelineParser(
                parametersParser,
                stepsParser,
                unknownNodeParser);

        return new YamlPipelineParser(
            pipelineParser);
    }
}
