using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class MapFieldHandlerTests
{
    [Fact]
    public void ReadMap_WithEmptyMap_ReturnsEmptyDictionary()
    {
        // Arrange
        var handler = new TestMapFieldHandler(new InterpolationParser());
        var reader = CreateReader("env: {}");

        _ = reader.Read<Scalar>();

        // Act
        var result = handler.ReadMapForTest(reader);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ReadMap_WithDuplicateKeys_UsesLastValue()
    {
        // Arrange
        var handler = new TestMapFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            env:
              KEY: first
              KEY: second
            """
        );

        _ = reader.Read<Scalar>();

        // Act
        var result = handler.ReadMapForTest(reader);

        // Assert
        Assert.Single(result);

        var keyValue = Assert.IsType<InterpolatedStringNode>(result["KEY"]);
        Assert.Equal("second", keyValue.Value);
    }

    [Fact]
    public void ReadMap_WithNonScalarKey_ThrowsYamlParseException()
    {
        // Arrange
        var handler = new TestMapFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            env:
              ? [a, b]
              : value
            """
        );

        _ = reader.Read<Scalar>();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => handler.ReadMapForTest(reader));

        Assert.Contains("Expected Scalar", exception.Message);
    }

    [Fact]
    public void ReadMap_WithNonScalarValue_ThrowsYamlParseException()
    {
        // Arrange
        var handler = new TestMapFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            env:
              KEY:
                nested: value
            """
        );

        _ = reader.Read<Scalar>();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => handler.ReadMapForTest(reader));

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

    private sealed class TestMapFieldHandler(InterpolationParser parser)
        : MapFieldHandler(parser)
    {
        public override string Key => "test";

        public override StepFields Apply(YamlReader reader, StepFields fields)
        {
            return fields;
        }

        public IReadOnlyDictionary<string, InterpolatedStringNode> ReadMapForTest(YamlReader reader)
        {
            return ReadMap(reader);
        }
    }
}
