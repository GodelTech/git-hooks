using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Builders;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Builders;

public class TemplateStepNodeBuilderTests
{
    [Fact]
    public void CanBuild_TemplatePresent_ReturnsTrue()
    {
        var fields = new StepFields
        {
            Template = "templates/build.yml"
        };

        var builder = new TemplateStepNodeBuilder();

        var result = builder.CanBuild(fields);

        Assert.True(result);
    }

    [Fact]
    public void CanBuild_TemplateMissing_ReturnsFalse()
    {
        var fields = new StepFields();
        var builder = new TemplateStepNodeBuilder();

        var result = builder.CanBuild(fields);

        Assert.False(result);
    }

    [Fact]
    public void Build_ValidTemplateFields_ReturnsTemplateStepNodeWithMappedProperties()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var unknownField = new UnknownFieldNode(
            new UnknownScalarNode("custom", span),
            new UnknownScalarNode("value", span),
            span
        );

        var fields = new StepFields
        {
            Template = "templates/build.yml",
            Parameters = new Dictionary<string, InterpolatedStringNode>
            {
                ["configuration"] = new InterpolatedStringNode("Release")
            },
            UnknownFields = [unknownField]
        };

        var builder = new TemplateStepNodeBuilder();

        var step = Assert.IsType<TemplateStepNode>(builder.Build(fields, span));

        Assert.Equal("templates/build.yml", step.Template);
        Assert.Equal("Release", step.Parameters["configuration"].Value);
        Assert.Single(step.UnknownFields);
        Assert.Equal(span, step.Span);
    }

    [Fact]
    public void Build_MissingTemplate_ThrowsInvalidOperationException()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields();
        var builder = new TemplateStepNodeBuilder();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build(fields, span));

        Assert.Equal("Template step builder requires a template value.", exception.Message);
    }

    [Fact]
    public void Build_ScriptFieldPresent_ThrowsYamlParseException()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Template = "templates/build.yml",
            Script = new InterpolatedStringNode("echo hello")
        };

        var builder = new TemplateStepNodeBuilder();

        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, span));

        Assert.Equal("Template step cannot contain fields: script", exception.Message);
    }
}
