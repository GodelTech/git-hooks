using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline;

public class StepParserTests
{
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
