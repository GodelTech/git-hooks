using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Builders;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Builders;

public class ScriptStepNodeBuilderTests
{
    [Fact]
    public void CanBuild_ScriptPresent_ReturnsTrue()
    {
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello")
        };

        var builder = new ScriptStepNodeBuilder();

        var result = builder.CanBuild(fields);

        Assert.True(result);
    }

    [Fact]
    public void CanBuild_ScriptMissing_ReturnsFalse()
    {
        var fields = new StepFields();
        var builder = new ScriptStepNodeBuilder();

        var result = builder.CanBuild(fields);

        Assert.False(result);
    }

    [Fact]
    public void Build_ValidScriptFields_ReturnsScriptStepNodeWithMappedProperties()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var condition = new RawExpressionNode("succeeded()");
        var unknownField = new UnknownFieldNode(
            new UnknownScalarNode("custom", span),
            new UnknownScalarNode("value", span),
            span
        );

        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            DisplayName = "Build",
            Condition = condition,
            TimeoutInMinutes = 10,
            WorkingDirectory = new InterpolatedStringNode("./src"),
            Env = new Dictionary<string, InterpolatedStringNode>
            {
                ["DOTNET_ENVIRONMENT"] = new InterpolatedStringNode("CI")
            },
            UnknownFields = [unknownField]
        };

        var builder = new ScriptStepNodeBuilder();

        var step = Assert.IsType<ScriptStepNode>(builder.Build(fields, span));

        Assert.Equal("echo hello", step.Script.Value);
        Assert.Equal("Build", step.DisplayName);
        Assert.Same(condition, step.Condition);
        Assert.Equal(10, step.TimeoutInMinutes);
        Assert.Equal("./src", step.WorkingDirectory?.Value);
        Assert.Equal("CI", step.Env["DOTNET_ENVIRONMENT"].Value);
        Assert.Single(step.UnknownFields);
        Assert.Equal(span, step.Span);
    }

    [Fact]
    public void Build_MissingScript_ThrowsInvalidOperationException()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields();
        var builder = new ScriptStepNodeBuilder();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build(fields, span));

        Assert.Equal("Script step builder requires a script value.", exception.Message);
    }

    [Fact]
    public void Build_TemplateFieldPresent_ThrowsYamlParseException()
    {
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            Template = "templates/build.yml"
        };

        var builder = new ScriptStepNodeBuilder();

        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, span));

        Assert.Equal("Script step cannot contain fields: template", exception.Message);
    }
}
