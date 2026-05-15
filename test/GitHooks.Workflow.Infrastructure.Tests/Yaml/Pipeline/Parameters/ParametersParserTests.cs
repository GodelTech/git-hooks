using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters;

public class ParametersParserTests
{
    [Fact]
    public void Parse_WithEmptySequence_ReturnsEmptyList()
    {
        var result = ParseParameters("[]");

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_WithSingleParameter_ReturnsSingleParameterNode()
    {
        var result = ParseParameters(
            """
            - name: configuration
            """
        );

        var parameter = Assert.Single(result);

        Assert.Equal("configuration", parameter.Name);
    }

    [Fact]
    public void Parse_WithMultipleParameters_ReturnsAllParameterNodes()
    {
        var result = ParseParameters(
            """
            - name: configuration
            - name: operatingSystem
            - name: retryCount
            """
        );

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Parse_WithMultipleParameters_PreservesOrder()
    {
        var result = ParseParameters(
            """
            - name: first
            - name: second
            """
        );

        Assert.Equal(2, result.Count);
        Assert.Equal("first", result[0].Name);
        Assert.Equal("second", result[1].Name);
    }

    [Fact]
    public void Parse_WithFullySpecifiedParameter_MapsAllFields()
    {
        var result = ParseParameters(
            """
            - name: operatingSystem
              displayName: Operating System
              type: string
              default: ubuntu
              values:
                - ubuntu
                - windows
            """
        );

        var parameter = Assert.Single(result);

        Assert.Equal("operatingSystem", parameter.Name);
        Assert.Equal("Operating System", parameter.DisplayName);
        Assert.Equal(ParameterType.Text, parameter.Type);
        Assert.Equal("ubuntu", parameter.Default);
        Assert.Equal(["ubuntu", "windows"], parameter.Values);
    }

    private static IReadOnlyList<ParameterNode> ParseParameters(string yamlParameters)
    {
        var reader = YamlReader.Create(yamlParameters, "parameters.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();

        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<ParametersParser>();

        var parameters = parser.Parse(reader);

        reader.Read<DocumentEnd>();
        reader.Read<StreamEnd>();

        return parameters;
    }
}
