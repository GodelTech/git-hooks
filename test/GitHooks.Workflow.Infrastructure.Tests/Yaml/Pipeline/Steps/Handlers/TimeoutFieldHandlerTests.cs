using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class TimeoutFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsTimeoutInMinutes()
    {
        var handler = new TimeoutFieldHandler();

        var key = handler.Key;

        Assert.Equal("timeoutInMinutes", key);
    }

    [Fact]
    public void Apply_WithIntegerScalar_SetsTimeoutInMinutes()
    {
        var handler = new TimeoutFieldHandler();
        var reader = CreateReader("timeoutInMinutes: 15");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new StepFields());

        Assert.Equal(15, result.TimeoutInMinutes);
    }

    [Fact]
    public void Apply_WithNonIntegerScalar_ThrowsFormatException()
    {
        var handler = new TimeoutFieldHandler();
        var reader = CreateReader("timeoutInMinutes: abc");

        _ = reader.Read<Scalar>();

        _ = Assert.Throws<FormatException>(() => handler.Apply(reader, new StepFields()));
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsPipelineParsingException()
    {
        var handler = new TimeoutFieldHandler();
        var reader = CreateReader(
            """
            timeoutInMinutes:
              nested: value
            """
        );

        _ = reader.Read<Scalar>();

        var exception = Assert.Throws<PipelineParsingException>(() => handler.Apply(reader, new StepFields()));

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
