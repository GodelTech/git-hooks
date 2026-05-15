using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class WorkingDirectoryFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsWorkingDirectory()
    {
        var handler = new WorkingDirectoryFieldHandler(new InterpolationParser());

        var key = handler.Key;

        Assert.Equal("workingDirectory", key);
    }

    [Fact]
    public void Apply_WithScalarValue_SetsWorkingDirectory()
    {
        var handler = new WorkingDirectoryFieldHandler(new InterpolationParser());
        var reader = CreateReader("workingDirectory: src/GitHooks");

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new StepFields());

        var workingDirectory = Assert.IsType<InterpolatedStringNode>(result.WorkingDirectory);
        Assert.Equal("src/GitHooks", workingDirectory.Value);
    }

    [Fact]
    public void Apply_WithNonScalarValue_ThrowsYamlParseException()
    {
        var handler = new WorkingDirectoryFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            workingDirectory:
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
