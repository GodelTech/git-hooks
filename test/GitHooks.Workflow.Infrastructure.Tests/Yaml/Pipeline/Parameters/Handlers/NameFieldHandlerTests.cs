using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters.Handlers;

public class NameFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsName()
    {
        var handler = new NameFieldHandler();

        var key = handler.Key;

        Assert.Equal("name", key);
    }

    [Fact]
    public void Apply_WithScalarValue_SetsName()
    {
        var handler = new NameFieldHandler();
        var reader = CreateReader("name: configuration");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Equal("configuration", result.Name);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        var handler = new NameFieldHandler();
        var reader = CreateReader(
            """
            name:
              nested: value
            """
        );

        _ = reader.Read<Scalar>();

        var exception = Assert.Throws<YamlParseException>(() => handler.Apply(reader, new ParameterFields()));

        Assert.Contains("Expected Scalar", exception.Message);
    }

    private static YamlReader CreateReader(string yaml)
    {
        var reader = YamlReader.Create(yaml, "parameter.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();
        reader.Read<MappingStart>();

        return reader;
    }
}
