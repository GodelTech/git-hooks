using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps;

public class StepParserTests
{
    [Fact]
    public void Parse_TemplateStep_ParsesTemplateAndParameters()
    {
        // Arrange & Act
        var result = ParseStep(
            """
            template: templates/build.yml
            parameters:
              configuration: Release
            """
        );

        // Assert
        var step = Assert.IsType<TemplateStepNode>(result);

        Assert.Equal("templates/build.yml", step.Template);
        Assert.Equal("Release", step.Parameters["configuration"].Value);
    }

    [Fact]
    public void Parse_ScriptStep_ParsesKnownFieldAndCapturesUnknownField()
    {
        // Arrange & Act
        var result = ParseStep(
            """
            script: echo hello
            list:
              - one
              - two
            """
        );

        // Assert
        var step = Assert.IsType<ScriptStepNode>(result);
        var unknownField = Assert.Single(step.UnknownFields);
        var key = Assert.IsType<UnknownScalarNode>(unknownField.Key);
        var value = Assert.IsType<UnknownSequenceNode>(unknownField.Value);

        Assert.Equal("echo hello", step.Script.Value);
        Assert.Equal("list", key.Value);
        Assert.Equal(2, value.Items.Count);
    }

    [Fact]
    public void Parse_ComplexUnknownKey_CapturesUnknownFieldWithSequenceKey()
    {
        // Arrange & Act
        var result = ParseStep(
            """
            script: echo hello
            ? [a, b]
            : custom
            """
        );

        // Assert
        var step = Assert.IsType<ScriptStepNode>(result);
        var unknownField = Assert.Single(step.UnknownFields);

        _ = Assert.IsType<UnknownSequenceNode>(unknownField.Key);

        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("custom", value.Value);
    }

    [Fact]
    public void Parse_WithoutScriptOrTemplate_ThrowsYamlParseException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<YamlParseException>(
            () => ParseStep(
                """
                displayName: build
                """
            )
        );

        Assert.Equal("Step must contain 'script' or 'template'", exception.Message);
    }

    [Fact]
    public void Parse_WithScriptAndTemplate_ThrowsYamlParseException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<YamlParseException>(
            () => ParseStep(
                """
                script: echo hello
                template: templates/build.yml
                """
            )
        );

        Assert.Equal("Step can contain only one of 'script' or 'template'", exception.Message);
    }

    private static StepNode ParseStep(string yamlStep)
    {
        var yaml = $"""
        {yamlStep}
        """;

        var reader = YamlReader.Create(yaml, "step.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();

        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<StepParser>();

        var step = parser.Parse(reader);

        reader.Read<DocumentEnd>();
        reader.Read<StreamEnd>();

        return step;
    }
}





