using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class ScriptFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsScript()
    {
        // Arrange
        var handler = new ScriptFieldHandler(new InterpolationParser());

        // Act
        var key = handler.Key;

        // Assert
        Assert.Equal("script", key);
    }

    [Fact]
    public void Apply_WithScalarValue_SetsScript()
    {
        // Arrange
        var handler = new ScriptFieldHandler(new InterpolationParser());
        var reader = CreateReader("script: echo hello");

        _ = reader.Read<Scalar>();

        // Act
        var result = handler.Apply(reader, new StepFields());

        // Assert
        var script = Assert.IsType<InterpolatedStringNode>(result.Script);
        Assert.Equal("echo hello", script.Value);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        // Arrange
        var handler = new ScriptFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            script:
              nested: value
            """
        );

        _ = reader.Read<Scalar>();

        // Act & Assert
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
