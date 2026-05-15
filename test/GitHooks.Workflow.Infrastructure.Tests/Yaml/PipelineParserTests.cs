using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml;

public class PipelineParserTests
{
    [Fact]
    public void Parse_WithValidPipeline_ReturnsPipelineNodeFromSource()
    {
        var parser = CreateParser();

        var result = parser.Parse(
            """
            steps:
              - script: echo hello
            """,
            "pipeline.yml"
        );

        Assert.Single(result.Steps);
        Assert.Equal("pipeline.yml", result.Span.Source.Name);
    }

    [Fact]
    public void Parse_WithMultipleDocuments_ThrowsYamlParseException()
    {
        var parser = CreateParser();

        var exception = Assert.Throws<YamlParseException>(
            () => parser.Parse(
                """
                steps:
                  - script: echo hello
                ---
                steps:
                  - script: echo again
                """,
                "pipeline.yml"
            )
        );

        Assert.Contains("Expected StreamEnd", exception.Message);
    }

    private static IPipelineParser CreateParser()
    {
        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IPipelineParser>();
    }
}
