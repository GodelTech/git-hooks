using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Handlers;

public class EnvFieldHandlerTests
{
    [Fact]
    public void Key_Always_ReturnsEnv()
    {
        var handler = new EnvFieldHandler(new InterpolationParser());

        var key = handler.Key;

        Assert.Equal("env", key);
    }

    [Fact]
    public void Apply_WithMappingValue_SetsEnvDictionary()
    {
        var handler = new EnvFieldHandler(new InterpolationParser());
        var reader = CreateReader(
            """
            env:
              BUILD_CONFIGURATION: Release
              ARTIFACT_NAME: app.zip
            """
        );

        _ = reader.Read<Scalar>();

        var result = handler.Apply(reader, new StepFields());

        Assert.Equal(2, result.Env.Count);

        var buildConfiguration = Assert.IsType<InterpolatedStringNode>(result.Env["BUILD_CONFIGURATION"]);
        var artifactName = Assert.IsType<InterpolatedStringNode>(result.Env["ARTIFACT_NAME"]);

        Assert.Equal("Release", buildConfiguration.Value);
        Assert.Equal("app.zip", artifactName.Value);
    }

    [Fact]
    public void Apply_WithScalarValue_ThrowsYamlParseException()
    {
        var handler = new EnvFieldHandler(new InterpolationParser());
        var reader = CreateReader("env: value");

        _ = reader.Read<Scalar>();

        var exception = Assert.Throws<YamlParseException>(() => handler.Apply(reader, new StepFields()));

        Assert.Contains("Expected MappingStart", exception.Message);
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
