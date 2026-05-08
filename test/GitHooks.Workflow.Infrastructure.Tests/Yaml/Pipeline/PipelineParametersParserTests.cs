using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline;

public class PipelineParametersParserTests
{
    [Fact]
    public void Parse_SingleStringParameter_ReturnsSingleDefinition()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: myParam
                displayName: 'My param'
                type: string
                default: hello
            steps:
              - script: echo ${{ parameters.myParam }}
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal("myParam", param.Name);
        Assert.Equal("My param", param.DisplayName);
        Assert.Equal(ParameterType.Text, param.Type);
        Assert.Equal("hello", param.Default);
        Assert.Empty(param.Values);
    }

    [Fact]
    public void Parse_BooleanParameter_ParsesTypeCorrectly()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: enableDebug
                type: boolean
                default: true
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal("enableDebug", param.Name);
        Assert.Equal(ParameterType.Boolean, param.Type);
        Assert.Equal("true", param.Default);
    }

    [Fact]
    public void Parse_NumberParameter_ParsesTypeCorrectly()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: retryCount
                type: number
                default: 3
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal(ParameterType.Number, param.Type);
        Assert.Equal("3", param.Default);
    }

    [Fact]
    public void Parse_ObjectParameter_ParsesTypeCorrectly()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: config
                type: object
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal(ParameterType.Mapping, param.Type);
    }

    [Fact]
    public void Parse_UnknownTypeString_FallsBackToText()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: myParam
                type: filePath
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal(ParameterType.Text, param.Type);
    }

    [Fact]
    public void Parse_ParameterWithoutType_DefaultsToText()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: myParam
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal(ParameterType.Text, param.Type);
    }

    [Fact]
    public void Parse_ParameterWithoutDefault_DefaultIsNull()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: requiredParam
                type: string
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Null(param.Default);
    }

    [Fact]
    public void Parse_ParameterWithValues_PopulatesValuesList()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: vmImage
                type: string
                default: ubuntu-latest
                values:
                  - windows-latest
                  - ubuntu-latest
                  - macOS-latest
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Equal(["windows-latest", "ubuntu-latest", "macOS-latest"], param.Values);
        Assert.Equal("ubuntu-latest", param.Default);
    }

    [Fact]
    public void Parse_MultipleParameters_ReturnsAllInOrder()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: first
                type: string
              - name: second
                type: boolean
                default: false
              - name: third
                type: number
                default: 42
            steps:
              - script: echo ok
            """
        );

        // Assert
        Assert.Equal(3, node.Parameters.Count);
        Assert.Equal("first", node.Parameters[0].Name);
        Assert.Equal("second", node.Parameters[1].Name);
        Assert.Equal("third", node.Parameters[2].Name);
    }

    [Fact]
    public void Parse_ParameterWithoutDisplayName_DisplayNameIsNull()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: noLabel
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);

        Assert.Null(param.DisplayName);
    }

    [Fact]
    public void Parse_ParameterMissingName_ThrowsYamlParseException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<YamlParseException>(
            () => ParseRoot(
                """
                parameters:
                  - type: string
                    default: value
                steps:
                  - script: echo ok
                """
            )
        );

        Assert.Equal("Parameter definition must contain 'name'", exception.Message);
    }

    [Fact]
    public void Parse_ParameterWithUnknownField_CapturesUnknownField()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: myParam
                futureAttribute: someValue
            steps:
              - script: echo ok
            """
        );

        // Assert
        var param = Assert.Single(node.Parameters);
        var unknownField = Assert.Single(param.UnknownFields);
        var key = Assert.IsType<UnknownScalarNode>(unknownField.Key);
        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("myParam", param.Name);
        Assert.Equal("futureAttribute", key.Value);
        Assert.Equal("someValue", value.Value);
    }

    [Fact]
    public void Parse_PipelineWithoutParameters_ParametersCollectionIsEmpty()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            steps:
              - script: echo ok
            """
        );

        // Assert
        Assert.Empty(node.Parameters);
    }

    [Fact]
    public void Parse_ParametersSection_NotCapturedAsUnknownField()
    {
        // Arrange & Act
        var node = ParseRoot(
            """
            parameters:
              - name: myParam
            steps:
              - script: echo ok
            """
        );

        // Assert: parameters must be a first-class field, not fall into unknown fields
        Assert.Empty(node.UnknownFields);
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
