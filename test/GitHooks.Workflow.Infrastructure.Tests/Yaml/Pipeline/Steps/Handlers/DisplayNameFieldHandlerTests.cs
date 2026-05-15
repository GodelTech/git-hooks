using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class DisplayNameFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsDisplayName()
    {
        var handler = new DisplayNameFieldHandler();

        var key = handler.Key;

        Assert.Equal("displayName", key);
    }

    [Fact]
    public void Apply_WithScalarValue_SetsDisplayName()
    {
        var handler = new DisplayNameFieldHandler();
        var reader = CreateReader("displayName: Build project");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new StepFields());

        Assert.Equal("Build project", result.DisplayName);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        var handler = new DisplayNameFieldHandler();
        var reader = CreateReader(
            """
            displayName:
              nested: value
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
