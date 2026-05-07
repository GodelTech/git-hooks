using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline;

public class StepParserTests
{
    [Fact]
    public void Parse_ScriptStep()
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

        Assert.Equal("echo hello", step.Script.Value);
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
