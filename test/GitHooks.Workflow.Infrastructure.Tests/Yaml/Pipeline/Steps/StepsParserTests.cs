using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps;

public class StepsParserTests
{
    [Fact]
    public void Parse_WithEmptySequence_ReturnsEmptyList()
    {
        var result = ParseSteps("[]");

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_WithSingleStep_ReturnsSingleStepNode()
    {
        var result = ParseSteps(
            """
            - script: echo hello
            """
        );

        var step = Assert.Single(result);

        _ = Assert.IsType<ScriptStepNode>(step);
    }

    [Fact]
    public void Parse_WithMultipleSteps_ReturnsAllStepNodes()
    {
        var result = ParseSteps(
            """
            - script: echo first
            - script: echo second
            - script: echo third
            """
        );

        Assert.Equal(3, result.Count);

        Assert.All(result, step => Assert.IsType<ScriptStepNode>(step));
    }

    [Fact]
    public void Parse_WithMultipleSteps_PreservesOrder()
    {
        var result = ParseSteps(
            """
            - script: echo first
            - script: echo second
            """
        );

        Assert.Equal(2, result.Count);

        var first = Assert.IsType<ScriptStepNode>(result[0]);
        var second = Assert.IsType<ScriptStepNode>(result[1]);

        Assert.Equal("echo first", first.Script.Value);
        Assert.Equal("echo second", second.Script.Value);
    }

    private static IReadOnlyList<StepNode> ParseSteps(string yamlSteps)
    {
        var yaml = $"""
        {yamlSteps}
        """;

        var reader = YamlReader.Create(yaml, "steps.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();

        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<StepsParser>();

        var steps = parser.Parse(reader);

        reader.Read<DocumentEnd>();
        reader.Read<StreamEnd>();

        return steps;
    }
}
