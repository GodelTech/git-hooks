using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Builders;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Parameters.Builders;

public class ParameterNodeBuilderTests
{
    [Fact]
    public void CanBuild_NamePresent_ReturnsTrue()
    {
        var fields = new ParameterFields
        {
            Name = "configuration"
        };

        var builder = new ParameterNodeBuilder();

        var result = builder.CanBuild(fields);

        Assert.True(result);
    }

    [Fact]
    public void CanBuild_NameMissing_ReturnsFalse()
    {
        var fields = new ParameterFields();
        var builder = new ParameterNodeBuilder();

        var result = builder.CanBuild(fields);

        Assert.False(result);
    }

    [Fact]
    public void Build_NameMissing_ThrowsYamlParseException()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new ParameterFields();
        var builder = new ParameterNodeBuilder();

        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));

        Assert.Equal("Parameter definition must contain 'name'", exception.Message);
        Assert.Equal(span, exception.Span);
    }

    [Fact]
    public void Build_ValidFields_ReturnsParameterNodeWithMappedProperties()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var unknownField = new UnknownFieldNode(
            new UnknownScalarNode("custom", span),
            new UnknownScalarNode("value", span),
            span
        );

        var fields = new ParameterFields
        {
            Name = "configuration",
            DisplayName = "Build configuration",
            Type = ParameterType.Text,
            DefaultValue = "Release",
            Values = ["Debug", "Release"]
        };

        var builder = new ParameterNodeBuilder();

        var parameter = builder.Build(fields, [unknownField], span);

        Assert.Equal("configuration", parameter.Name);
        Assert.Equal("Build configuration", parameter.DisplayName);
        Assert.Equal(ParameterType.Text, parameter.Type);
        Assert.Equal("Release", parameter.Default);
        Assert.Equal(["Debug", "Release"], parameter.Values);
        Assert.Single(parameter.UnknownFields);
        Assert.Equal(span, parameter.Span);
    }
}
