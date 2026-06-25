using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

namespace GitHooks.Infrastructure.Yaml.Tests;

internal static class TestParserFactory
{
    public static YamlPipelineParser CreateYamlPipelineParser()
    {
        // expressions
        var expressionParser = CreateExpressionParser();

        // values
        var fieldValueParser = CreateFieldValueParser();

        // fields
        var fieldParser = CreateFieldParser(
            expressionParser,
            fieldValueParser);

        // parameters
        var parameterParser = CreateParameterParser(
            fieldParser,
            fieldValueParser);

        var parametersParser = CreateParametersParser(
            parameterParser);

        // steps
        var stepParser = CreateStepParser(
            fieldParser,
            fieldValueParser);

        var stepsParser = CreateStepsParser(
            stepParser);

        // pipeline
        var pipelineParser = CreatePipelineParser(
            parametersParser,
            stepsParser,
            fieldValueParser);

        return new YamlPipelineParser(
            pipelineParser);
    }

    public static PipelineParser CreatePipelineParser(
        ParametersParser? parametersParser = null,
        StepsParser? stepsParser = null,
        FieldValueParser? fieldValueParser = null)
    {
        return new PipelineParser(
            parametersParser ?? CreateParametersParser(),
            stepsParser ?? CreateStepsParser(),
            fieldValueParser ?? CreateFieldValueParser());
    }

    public static ParametersParser CreateParametersParser(
        ParameterParser? parameterParser = null)
    {
        return new ParametersParser(
            parameterParser ?? CreateParameterParser());
    }

    public static ParameterParser CreateParameterParser(
        FieldParser? fieldParser = null,
        FieldValueParser? fieldValueParser = null)
    {
        return new ParameterParser(
            fieldParser ?? CreateFieldParser(),
            fieldValueParser ?? CreateFieldValueParser());
    }

    public static StepsParser CreateStepsParser(
        StepParser? stepParser = null)
    {
        return new StepsParser(
            stepParser ?? CreateStepParser());
    }

    public static StepParser CreateStepParser(
        FieldParser? fieldParser = null,
        FieldValueParser? fieldValueParser = null)
    {
        return new StepParser(
            fieldParser ?? CreateFieldParser(),
            fieldValueParser ?? CreateFieldValueParser());
    }

    public static FieldParser CreateFieldParser(
        ExpressionParser? expressionParser = null,
        FieldValueParser? fieldValueParser = null)
    {
        return new FieldParser(
            expressionParser ?? CreateExpressionParser(),
            fieldValueParser ?? CreateFieldValueParser());
    }

    public static FieldValueParser CreateFieldValueParser()
    {
        return new FieldValueParser();
    }

    public static ExpressionParser CreateExpressionParser()
    {
        var variableExpressionParser = CreateVariableExpressionParser();

        var interpolatedStringParser = CreateInterpolatedStringParser(
            variableExpressionParser);

        return new ExpressionParser(
            interpolatedStringParser);
    }

    public static VariableExpressionParser CreateVariableExpressionParser()
    {
        return new VariableExpressionParser();
    }

    public static InterpolatedStringParser CreateInterpolatedStringParser(
        VariableExpressionParser? variableExpressionParser = null)
    {
        return new InterpolatedStringParser(
            variableExpressionParser ?? CreateVariableExpressionParser());
    }

    public static FieldTracker CreateFieldTracker(
        FieldValueParser? fieldValueParser = null)
    {
        return new FieldTracker(
            fieldValueParser ?? CreateFieldValueParser());
    }
}
