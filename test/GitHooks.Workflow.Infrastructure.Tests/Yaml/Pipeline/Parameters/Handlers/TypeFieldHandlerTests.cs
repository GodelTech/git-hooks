using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters.Handlers;

public class TypeFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsType()
    {
        var handler = new TypeFieldHandler();

        var key = handler.Key;

        Assert.Equal("type", key);
    }

    [Theory]
    [InlineData("boolean", ParameterType.Boolean)]
    [InlineData("number", ParameterType.Number)]
    [InlineData("object", ParameterType.Mapping)]
    [InlineData("string", ParameterType.Text)]
    [InlineData("unknown", ParameterType.Text)]
    public void Apply_WithKnownTypeString_SetsType(string typeValue, ParameterType expectedType)
    {
        var handler = new TypeFieldHandler();
        var reader = CreateReader($"type: {typeValue}");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Equal(expectedType, result.Type);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        var handler = new TypeFieldHandler();
        var reader = CreateReader(
            """
            type:
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
