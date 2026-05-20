using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Infrastructure.DependencyInjection;

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
    public void Parse_WithParameters_ReturnsPipelineNodeWithParsedParameters()
    {
        var parser = CreateParser();

        var result = parser.Parse(
            """
            parameters:
              - name: configuration
                type: string
                default: Release
            steps:
              - script: echo hello
            """,
            "pipeline.yml"
        );

        var parameter = Assert.Single(result.Parameters);

        Assert.Equal("configuration", parameter.Name);
        Assert.Equal("Release", parameter.Default);
    }

    [Fact]
    public void Parse_WithMultipleDocuments_ThrowsPipelineParsingException()
    {
        var parser = CreateParser();

        var exception = Assert.Throws<PipelineParsingException>(
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
