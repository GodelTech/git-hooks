using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters.Handlers;

public class ValuesFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsValues()
    {
        var handler = new ValuesFieldHandler();

        var key = handler.Key;

        Assert.Equal("values", key);
    }

    [Fact]
    public void Apply_WithSequenceValue_SetsValuesList()
    {
        var handler = new ValuesFieldHandler();
        var reader = CreateReader(
            """
            values:
              - Debug
              - Release
            """
        );

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Equal(2, result.Values.Count);
        Assert.Equal("Debug", result.Values[0]);
        Assert.Equal("Release", result.Values[1]);
    }

    [Fact]
    public void Apply_WithEmptySequence_SetsEmptyValuesList()
    {
        var handler = new ValuesFieldHandler();
        var reader = CreateReader("values: []");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new ParameterFields());

        Assert.Empty(result.Values);
    }

    [Fact]
    public void Apply_WithScalarValue_ThrowsPipelineParsingException()
    {
        var handler = new ValuesFieldHandler();
        var reader = CreateReader("values: Debug");

        _ = reader.Read<Scalar>();

        var exception = Assert.Throws<PipelineParsingException>(() => handler.Apply(reader, new ParameterFields()));

        Assert.Contains("Expected SequenceStart", exception.Message);
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
