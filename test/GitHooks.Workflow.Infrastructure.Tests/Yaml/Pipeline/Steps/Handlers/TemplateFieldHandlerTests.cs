using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class TemplateFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsTemplate()
    {
        var handler = new TemplateFieldHandler();

        var key = handler.Key;

        Assert.Equal("template", key);
    }

    [Fact]
    public void Apply_WithScalarValue_SetsTemplate()
    {
        var handler = new TemplateFieldHandler();
        var reader = CreateReader("template: templates/build.yml");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new StepFields());

        Assert.Equal("templates/build.yml", result.Template);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsPipelineParsingException()
    {
        var handler = new TemplateFieldHandler();
        var reader = CreateReader(
            """
            template:
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
