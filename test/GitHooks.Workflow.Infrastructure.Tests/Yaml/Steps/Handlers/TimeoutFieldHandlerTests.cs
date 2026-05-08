using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Steps.Handlers;

public class TimeoutFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsTimeoutInMinutes()
    {
        // Arrange
        var handler = new TimeoutFieldHandler();

        // Act
        var key = handler.Key;

        // Assert
        Assert.Equal("timeoutInMinutes", key);
    }

    [Fact]
    public void Apply_WithIntegerScalar_SetsTimeoutInMinutes()
    {
        // Arrange
        var handler = new TimeoutFieldHandler();
        var reader = CreateReader("timeoutInMinutes: 15");

        _ = reader.Read<Scalar>();

        // Act
        var result = handler.Apply(reader, new StepFields());

        // Assert
        Assert.Equal(15, result.TimeoutInMinutes);
    }

    [Fact]
    public void Apply_WithNonIntegerScalar_ThrowsFormatException()
    {
        // Arrange
        var handler = new TimeoutFieldHandler();
        var reader = CreateReader("timeoutInMinutes: abc");

        _ = reader.Read<Scalar>();

        // Act & Assert
        _ = Assert.Throws<FormatException>(() => handler.Apply(reader, new StepFields()));
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        // Arrange
        var handler = new TimeoutFieldHandler();
        var reader = CreateReader(
            """
            timeoutInMinutes:
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
