using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline;

public class PipelineRootParserTests
{
    [Fact]
    public void Parse_WithoutSteps_ThrowsYamlParseException()
    {
        // Arrange & Act
        static void action()
        {
            ParseRoot(
                """
                name: ci
                """
            );
        }

        // Assert
        var exception = Assert.Throws<YamlParseException>(action);
        Assert.Equal("Pipeline must contain 'steps'", exception.Message);
    }

    [Fact]
    public void Parse_WithUnknownRootScalarField_CapturesUnknownField()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            name: ci
            steps:
              - script: echo hello
            """
        );

        // Assert
        var unknownField = Assert.Single(node.UnknownFields);
        var key = Assert.IsType<UnknownScalarNode>(unknownField.Key);
        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("name", key.Value);
        Assert.Equal("ci", value.Value);
    }

    [Fact]
    public void Parse_WithUnknownRootComplexKey_CapturesUnknownField()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            ? [a, b]
            : custom
            steps:
              - script: echo hello
            """
        );

        // Assert
        var unknownField = Assert.Single(node.UnknownFields);

        _ = Assert.IsType<UnknownSequenceNode>(unknownField.Key);

        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("custom", value.Value);
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
