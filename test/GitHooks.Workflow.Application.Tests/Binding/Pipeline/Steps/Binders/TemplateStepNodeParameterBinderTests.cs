using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Expressions;
using GitHooks.Workflow.Application.Binding.Pipeline.Steps.Binders;
using GitHooks.Workflow.Application.Binding.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Pipeline.Steps.Binders;

public sealed class TemplateStepNodeParameterBinderTests
{
    [Fact]
    public void CanBind_WithTemplateStep_ReturnsTrue()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var templateStep = new TemplateStepNode(
            Template: "template.yml",
            Parameters: new Dictionary<string, InterpolatedStringNode>(),
            UnknownFields: [],
            Span: span
        );

        var result = binder.CanBind(templateStep);

        Assert.True(result);
    }

    [Fact]
    public void CanBind_WithNonTemplateStep_ReturnsFalse()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var scriptStep = new ScriptStepNode(
            Script: new InterpolatedStringNode("echo hello"),
            UnknownFields: [],
            Span: span
        );

        var result = binder.CanBind(scriptStep);

        Assert.False(result);
    }

    [Fact]
    public void Bind_WithTemplateStep_RebindsTemplateParametersAndUnknownFields()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(
            ("templateName", "deploy.yml"),
            ("env", "stage"),
            ("region", "eu")
        );

        var templateStep = new TemplateStepNode(
            Template: "templates/${{ parameters.templateName }}",
            Parameters: new Dictionary<string, InterpolatedStringNode>
            {
                ["environment"] = new("${{ parameters.env }}"),
                ["location"] = new("${{ parameters.region }}")
            },
            UnknownFields:
            [
                new UnknownFieldNode(
                    new UnknownScalarNode("meta", span),
                    new UnknownScalarNode("${{ parameters.env }}", span),
                    span
                )
            ],
            Span: span
        );

        var result = binder.Bind(templateStep, context);

        var step = Assert.IsType<TemplateStepNode>(result);
        Assert.Equal("templates/deploy.yml", step.Template);
        Assert.Equal("stage", step.Parameters["environment"].Value);
        Assert.Equal("eu", step.Parameters["location"].Value);

        var unknownField = Assert.Single(step.UnknownFields);
        var unknownValue = Assert.IsType<UnknownScalarNode>(unknownField.Value);
        Assert.Equal("stage", unknownValue.Value);
    }

    [Fact]
    public void Bind_WithUnsupportedTemplateExpression_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(
            ("templateName", "deploy.yml"),
            ("env", "prod"),
            ("region", "eu")
        );

        var templateStep = new TemplateStepNode(
            Template: "templates/${{ variables.templateName }}",
            Parameters: new Dictionary<string, InterpolatedStringNode>(),
            UnknownFields: [],
            Span: span
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind(templateStep, context));

        Assert.Contains("Unsupported expression 'variables.templateName'", exception.Message, StringComparison.Ordinal);
    }

    private static TemplateStepNodeParameterBinder CreateBinder()
    {
        var stringParameterBinder = new StringParameterBinder();
        var interpolationParameterBinder = new InterpolationParameterBinder(stringParameterBinder);
        var unknownNodeParameterBinder = new UnknownNodeParameterBinder(stringParameterBinder);

        return new TemplateStepNodeParameterBinder(
            interpolationParameterBinder,
            stringParameterBinder,
            unknownNodeParameterBinder
        );
    }

    private static ParameterBindingContext CreateContext(params (string Name, string? DefaultValue)[] parameters)
    {
        var span = CreateSpan();

        var declarations = parameters
            .Select(parameter => new ParameterNode(parameter.Name, null, ParameterType.Text, parameter.DefaultValue, [], [], span))
            .ToList();

        return ParameterBindingContext.Create(declarations);
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("pipeline.yml"));
    }
}
