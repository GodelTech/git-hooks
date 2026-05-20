using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters.Handlers;

public class DefaultFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsDefault()
    {
        var handler = new DefaultFieldHandler();

        var key = handler.Key;

        Assert.Equal("default", key);
    }

    [Fact]
    public void Apply_WithStringScalar_SetsDefaultValue()
    {
        var handler = new DefaultFieldHandler();
        var reader = CreateReader("default: Release");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Equal("Release", result.DefaultValue);
    }

    [Fact]
    public void Apply_WithBooleanScalar_SetsDefaultValue()
    {
        var handler = new DefaultFieldHandler();
        var reader = CreateReader("default: true");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Equal("true", result.DefaultValue);
    }

    [Fact]
    public void Apply_WithNumericScalar_SetsDefaultValue()
    {
        var handler = new DefaultFieldHandler();
        var reader = CreateReader("default: 42");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Equal("42", result.DefaultValue);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsPipelineParsingException()
    {
        var handler = new DefaultFieldHandler();
        var reader = CreateReader(
            """
            default:
              nested: value
            """
        );

        _ = reader.Read<Scalar>();

        var exception = Assert.Throws<PipelineParsingException>(() => handler.Apply(reader, new ParameterFields()));

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
