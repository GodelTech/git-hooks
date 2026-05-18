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

public sealed class ScriptStepNodeParameterBinderTests
{
    [Fact]
    public void CanBind_WithScriptStep_ReturnsTrue()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var scriptStep = new ScriptStepNode(
            Script: new InterpolatedStringNode("echo hello"),
            UnknownFields: [],
            Span: span
        );

        var result = binder.CanBind(scriptStep);

        Assert.True(result);
    }

    [Fact]
    public void CanBind_WithNonScriptStep_ReturnsFalse()
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

        Assert.False(result);
    }

    [Fact]
    public void Bind_WithScriptStep_RebindsAllBindableFields()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(
            ("command", "build"),
            ("name", "Build Step"),
            ("workdir", "src"),
            ("nodeVersion", "20")
        );

        var scriptStep = new ScriptStepNode(
            Script: new InterpolatedStringNode("npm run ${{ parameters.command }}"),
            UnknownFields:
            [
                new UnknownFieldNode(
                    new UnknownScalarNode("meta", span),
                    new UnknownScalarNode("${{ parameters.name }}", span),
                    span
                )
            ],
            Span: span
        )
        {
            DisplayName = "${{ parameters.name }}",
            WorkingDirectory = new InterpolatedStringNode("/${{ parameters.workdir }}"),
            Env = new Dictionary<string, InterpolatedStringNode>
            {
                ["NODE_VERSION"] = new("${{ parameters.nodeVersion }}")
            }
        };

        var result = binder.Bind(scriptStep, context);

        var step = Assert.IsType<ScriptStepNode>(result);
        Assert.Equal("npm run build", step.Script.Value);
        Assert.Equal("Build Step", step.DisplayName);
        Assert.Equal("/src", step.WorkingDirectory?.Value);
        Assert.Equal("20", step.Env["NODE_VERSION"].Value);

        var unknownField = Assert.Single(step.UnknownFields);
        var unknownValue = Assert.IsType<UnknownScalarNode>(unknownField.Value);
        Assert.Equal("Build Step", unknownValue.Value);
    }

    [Fact]
    public void Bind_WithUnsupportedDisplayNameExpression_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("command", "build"));

        var scriptStep = new ScriptStepNode(
            Script: new InterpolatedStringNode("npm run ${{ parameters.command }}"),
            UnknownFields: [],
            Span: span
        )
        {
            DisplayName = "${{ variables.name }}"
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind(scriptStep, context));

        Assert.Contains("Unsupported expression 'variables.name'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Bind_WithNullOptionalFields_KeepsDisplayNameAndWorkingDirectoryNull()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("command", "test"));

        var scriptStep = new ScriptStepNode(
            Script: new InterpolatedStringNode("npm run ${{ parameters.command }}"),
            UnknownFields: [],
            Span: span
        )
        {
            DisplayName = null,
            WorkingDirectory = null,
            Env = new Dictionary<string, InterpolatedStringNode>()
        };

        var result = binder.Bind(scriptStep, context);

        var step = Assert.IsType<ScriptStepNode>(result);
        Assert.Equal("npm run test", step.Script.Value);
        Assert.Null(step.DisplayName);
        Assert.Null(step.WorkingDirectory);
        Assert.Empty(step.Env);
    }

    [Fact]
    public void Bind_WithUnsupportedEnvironmentExpression_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("command", "build"));

        var scriptStep = new ScriptStepNode(
            Script: new InterpolatedStringNode("npm run ${{ parameters.command }}"),
            UnknownFields: [],
            Span: span
        )
        {
            Env = new Dictionary<string, InterpolatedStringNode>
            {
                ["NODE_ENV"] = new("${{ variables.environment }}")
            }
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind(scriptStep, context));

        Assert.Contains("Unsupported expression 'variables.environment'", exception.Message, StringComparison.Ordinal);
    }

    private static ScriptStepNodeParameterBinder CreateBinder()
    {
        var stringParameterBinder = new StringParameterBinder();
        var interpolationParameterBinder = new InterpolationParameterBinder(stringParameterBinder);
        var unknownNodeParameterBinder = new UnknownNodeParameterBinder(stringParameterBinder);

        return new ScriptStepNodeParameterBinder(
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
