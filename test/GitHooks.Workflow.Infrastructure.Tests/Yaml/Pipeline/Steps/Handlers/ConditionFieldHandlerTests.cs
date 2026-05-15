using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class ConditionFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsCondition()
    {
        var handler = new ConditionFieldHandler(new ExpressionParser());

        var key = handler.Key;

        Assert.Equal("condition", key);
    }

    [Fact]
    public void Apply_WithScalarValue_ParsesAndSetsCondition()
    {
        var handler = new ConditionFieldHandler(new ExpressionParser());
        var reader = CreateReader("condition: '  succeeded()  '");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new StepFields());

        var condition = Assert.IsType<RawExpressionNode>(result.Condition);
        Assert.Equal("succeeded()", condition.Value);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        var handler = new ConditionFieldHandler(new ExpressionParser());
        var reader = CreateReader(
            """
            condition:
              - a
              - b
            """
        );

        _ = reader.Read<Scalar>();

        var exception = Assert.Throws<YamlParseException>(() => handler.Apply(reader, new StepFields()));

        Assert.Contains("Expected Scalar", exception.Message);
    }

    private static YamlReader CreateReader(string yaml)
    {
        var reader = YamlReader.Create(yaml, "step.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();
        reader.Read<MappingStart>();

        return reader;
    }
}
