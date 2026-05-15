using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline;

public class PipelineRootParserTests
{
    [Fact]
    public void Parse_WithoutSteps_ThrowsYamlParseException()
    {
        var exception = Assert.Throws<YamlParseException>(
            () => ParseRoot(
                """
                name: ci
                """
            )
        );

        Assert.Equal("Pipeline must contain 'steps'", exception.Message);
    }

    [Fact]
    public void Parse_WithUnknownRootScalarField_CapturesUnknownField()
    {
        var node = ParseRoot(
            """
            name: ci
            steps:
              - script: echo hello
            """
        );

        var unknownField = Assert.Single(node.UnknownFields);
        var key = Assert.IsType<UnknownScalarNode>(unknownField.Key);
        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("name", key.Value);
        Assert.Equal("ci", value.Value);
    }

    [Fact]
    public void Parse_WithUnknownRootComplexKey_CapturesUnknownField()
    {
        var node = ParseRoot(
            """
            ? [a, b]
            : custom
            steps:
              - script: echo hello
            """
        );

        var unknownField = Assert.Single(node.UnknownFields);

        _ = Assert.IsType<UnknownSequenceNode>(unknownField.Key);

        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("custom", value.Value);
    }

    [Fact]
    public void Parse_WithParametersBeforeSteps_ParsesRootParameters()
    {
        var node = ParseRoot(
            """
            parameters:
              - name: operatingSystem
                type: string
                default: ubuntu
            steps:
              - script: echo hello
            """
        );

        var parameter = Assert.Single(node.Parameters);

        Assert.Equal("operatingSystem", parameter.Name);
        Assert.Equal(ParameterType.Text, parameter.Type);
        Assert.Equal("ubuntu", parameter.Default);
    }

    private static PipelineNode ParseRoot(string yamlRoot)
    {
        var yaml = $"""
        {yamlRoot}
        """;

        var reader = YamlReader.Create(yaml, "pipeline.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();

        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<PipelineRootParser>();

        var pipeline = parser.Parse(reader);

        reader.Read<DocumentEnd>();
        reader.Read<StreamEnd>();

        return pipeline;
    }
}
