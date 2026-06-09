using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

namespace GitHooks.Infrastructure.Yaml.Tests;

internal static class TestParserFactory
{
    public static YamlParserCursor CreateCursor(
        string yaml,
        SourceDocument? sourceDocument = null)
    {
        return YamlParserCursor.Create(
            yaml,
            sourceDocument ?? new SourceDocument("test.yaml"));
    }

    public static YamlParserCursor CreateDummyCursor()
    {
        return CreateCursor(
            "{}",
            new SourceDocument("test.yaml"));
    }

    public static YamlPipelineParser CreateYamlPipelineParser()
    {
        // unknown
        var unknownNodeParser = CreateUnknownNodeParser();

        // expressions
        var expressionParser = CreateExpressionParser();

        // parameters
        var parameterParser = CreateParameterParser(
            expressionParser,
            unknownNodeParser);

        var parametersParser = CreateParametersParser(
            parameterParser);

        // steps
        var stepParser = CreateStepParser(
            expressionParser,
            unknownNodeParser);

        var stepsParser = CreateStepsParser(
            stepParser);

        // pipeline
        var pipelineParser = CreatePipelineParser(
            parametersParser,
            stepsParser,
            unknownNodeParser);

        return new YamlPipelineParser(
            pipelineParser);
    }

    public static PipelineParser CreatePipelineParser(
        ParametersParser? parametersParser = null,
        StepsParser? stepsParser = null,
        UnknownNodeParser? unknownNodeParser = null)
    {
        return new PipelineParser(
            parametersParser ?? CreateParametersParser(),
            stepsParser ?? CreateStepsParser(),
            unknownNodeParser ?? CreateUnknownNodeParser());
    }

    public static ParametersParser CreateParametersParser(
        ParameterParser? parameterParser = null)
    {
        return new ParametersParser(
            parameterParser ?? CreateParameterParser());
    }

    public static ParameterParser CreateParameterParser(
        ExpressionParser? expressionParser = null,
        UnknownNodeParser? unknownNodeParser = null)
    {
        return new ParameterParser(
            expressionParser ?? CreateExpressionParser(),
            unknownNodeParser ?? CreateUnknownNodeParser());
    }

    public static StepsParser CreateStepsParser(
        StepParser? stepParser = null)
    {
        return new StepsParser(
            stepParser ?? CreateStepParser());
    }

    public static StepParser CreateStepParser(
        ExpressionParser? expressionParser = null,
        UnknownNodeParser? unknownNodeParser = null)
    {
        return new StepParser(
            expressionParser ?? CreateExpressionParser(),
            unknownNodeParser ?? CreateUnknownNodeParser());
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

    public static UnknownNodeParser CreateUnknownNodeParser()
    {
        return new UnknownNodeParser();
    }

    public static ExpressionNode CreateExpression()
    {
        return new StringLiteralExpressionNode
        {
            Value = "test",
            Span = SourceSpan.Unknown
        };
    }
}
