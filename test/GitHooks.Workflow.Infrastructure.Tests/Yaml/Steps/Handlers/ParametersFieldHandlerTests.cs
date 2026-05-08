using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Steps.Handlers;

public class ParametersFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsParameters()
    {
        // Arrange
        var handler = new ParametersFieldHandler(new InterpolationParser());

        // Act
        var key = handler.Key;

        // Assert
        Assert.Equal("parameters", key);
    }

    [Fact]
    public void Apply_WithMappingValue_SetsParametersDictionary()
    {
        // Arrange
        var handler = new ParametersFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            parameters:
              solution: git-hooks.slnx
              configuration: Release
            """
        );

        _ = reader.Read<Scalar>();

        // Act
        var result = handler.Apply(reader, new StepFields());

        // Assert
        Assert.Equal(2, result.Parameters.Count);

        var solution = Assert.IsType<InterpolatedStringNode>(result.Parameters["solution"]);
        var configuration = Assert.IsType<InterpolatedStringNode>(result.Parameters["configuration"]);

        Assert.Equal("git-hooks.slnx", solution.Value);
        Assert.Equal("Release", configuration.Value);
    }

    [Fact]
    public void Apply_WithScalarValue_ThrowsYamlParseException()
    {
        // Arrange
        var handler = new ParametersFieldHandler(new InterpolationParser());
        var reader = CreateReader("parameters: value");

        _ = reader.Read<Scalar>();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => handler.Apply(reader, new StepFields()));

        Assert.Contains("Expected MappingStart", exception.Message);
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
