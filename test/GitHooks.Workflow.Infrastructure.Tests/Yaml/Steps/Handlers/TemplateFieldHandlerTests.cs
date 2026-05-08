using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Steps.Handlers;

public class TemplateFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsTemplate()
    {
        // Arrange
        var handler = new TemplateFieldHandler();

        // Act
        var key = handler.Key;

        // Assert
        Assert.Equal("template", key);
    }

    [Fact]
    public void Apply_WithScalarValue_SetsTemplate()
    {
        // Arrange
        var handler = new TemplateFieldHandler();
        var reader = CreateReader("template: templates/build.yml");

        _ = reader.Read<Scalar>();

        // Act
        var result = handler.Apply(reader, new StepFields());

        // Assert
        Assert.Equal("templates/build.yml", result.Template);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        // Arrange
        var handler = new TemplateFieldHandler();
        var reader = CreateReader(
            """
            template:
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
