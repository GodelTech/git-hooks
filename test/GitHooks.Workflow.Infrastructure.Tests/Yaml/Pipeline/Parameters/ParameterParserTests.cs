using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters;

public class ParameterParserTests
{
    [Fact]
    public void Parse_WithNameOnly_ReturnsParameterNodeWithDefaults()
    {
        var result = ParseParameter(
            """
            name: configuration
            """
        );

        Assert.Equal("configuration", result.Name);
        Assert.Null(result.DisplayName);
        Assert.Equal(ParameterType.Text, result.Type);
        Assert.Null(result.Default);
        Assert.Empty(result.Values);
        Assert.Empty(result.UnknownFields);
    }

    [Fact]
    public void Parse_WithAllKnownFields_MapsAllProperties()
    {
        var result = ParseParameter(
            """
            name: operatingSystem
            displayName: Operating System
            type: string
            default: ubuntu
            values:
              - ubuntu
              - windows
            """
        );

        Assert.Equal("operatingSystem", result.Name);
        Assert.Equal("Operating System", result.DisplayName);
        Assert.Equal(ParameterType.Text, result.Type);
        Assert.Equal("ubuntu", result.Default);
        Assert.Equal(["ubuntu", "windows"], result.Values);
    }

    [Fact]
    public void Parse_WithBooleanType_SetsBooleanParameterType()
    {
        var result = ParseParameter(
            """
            name: isEnabled
            type: boolean
            default: true
            """
        );

        Assert.Equal(ParameterType.Boolean, result.Type);
        Assert.Equal("true", result.Default);
    }

    [Fact]
    public void Parse_WithNumberType_SetsNumberParameterType()
    {
        var result = ParseParameter(
            """
            name: retryCount
            type: number
            default: 3
            """
        );

        Assert.Equal(ParameterType.Number, result.Type);
        Assert.Equal("3", result.Default);
    }

    [Fact]
    public void Parse_WithObjectType_SetsMappingParameterType()
    {
        var result = ParseParameter(
            """
            name: config
            type: object
            """
        );

        Assert.Equal(ParameterType.Mapping, result.Type);
    }

    [Fact]
    public void Parse_WithUnknownField_CapturesItInUnknownFields()
    {
        var result = ParseParameter(
            """
            name: configuration
            custom: value
            """
        );

        var unknownField = Assert.Single(result.UnknownFields);
        var key = Assert.IsType<UnknownScalarNode>(unknownField.Key);

        Assert.Equal("custom", key.Value);
    }

    [Fact]
    public void Parse_WithComplexUnknownKey_CapturesUnknownFieldWithSequenceKey()
    {
        var result = ParseParameter(
            """
            name: configuration
            ? [a, b]
            : custom
            """
        );

        var unknownField = Assert.Single(result.UnknownFields);

        _ = Assert.IsType<UnknownSequenceNode>(unknownField.Key);

        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);

        Assert.Equal("custom", value.Value);
    }

    [Fact]
    public void Parse_WithoutName_ThrowsYamlParseException()
    {
        var exception = Assert.Throws<YamlParseException>(
            () => ParseParameter(
                """
                displayName: Build configuration
                type: string
                """
            )
        );

        Assert.Equal("Parameter definition must contain 'name'", exception.Message);
    }

    private static ParameterNode ParseParameter(string yamlParameter)
    {
        var reader = YamlReader.Create(yamlParameter, "parameter.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();

        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<ParameterParser>();

        var parameter = parser.Parse(reader);

        reader.Read<DocumentEnd>();
        reader.Read<StreamEnd>();

        return parameter;
    }
}
