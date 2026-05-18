using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Pipeline;
using GitHooks.Workflow.Application.Binding.Pipeline.Steps;
using GitHooks.Workflow.Application.Binding.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding;

public sealed class PipelineParameterBinderTests
{
    [Fact]
    public void Bind_WithNullPipeline_ThrowsArgumentNullException()
    {
        var binder = CreateBinder();

        _ = Assert.Throws<ArgumentNullException>(() => binder.Bind(null!));
    }

    [Fact]
    public void Bind_WithoutOverrides_UsesDeclaredDefaultParameterValue()
    {
        var binder = CreateBinder();
        var pipeline = CreatePipelineWithOwnerParameter();

        var result = binder.Bind(pipeline);

        var unknownField = Assert.Single(result.UnknownFields);
        var unknownValue = Assert.IsType<UnknownScalarNode>(unknownField.Value);
        Assert.Equal("contoso", unknownValue.Value);
    }

    [Fact]
    public void Bind_WithOverrides_UsesOverrideParameterValue()
    {
        var binder = CreateBinder();
        var pipeline = CreatePipelineWithOwnerParameter();

        var result = binder.Bind(
            pipeline,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["owner"] = "fabrikam"
            }
        );

        var unknownField = Assert.Single(result.UnknownFields);
        var unknownValue = Assert.IsType<UnknownScalarNode>(unknownField.Value);
        Assert.Equal("fabrikam", unknownValue.Value);
    }

    [Fact]
    public void Bind_WithUndefinedOverrideParameter_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var pipeline = CreatePipelineWithOwnerParameter();

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => binder.Bind(
                pipeline,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["unknown"] = "value"
                }
            )
        );

        Assert.Contains("Parameter 'unknown' is not defined.", exception.Message, StringComparison.Ordinal);
    }

    private static PipelineParameterBinder CreateBinder()
    {
        var stepBinder = new FakeStepNodeParameterBinder();
        var stepsBinder = new StepsParameterBinder(new StepParameterBinder([stepBinder]));
        var unknownBinder = new UnknownNodeParameterBinder(new StringParameterBinder());
        var rootBinder = new PipelineRootParameterBinder(stepsBinder, unknownBinder);

        return new PipelineParameterBinder(rootBinder);
    }

    private static PipelineNode CreatePipelineWithOwnerParameter()
    {
        var span = CreateSpan();

        return new PipelineNode(
            Parameters:
            [
                new ParameterNode("owner", null, ParameterType.Text, "contoso", [], [], span)
            ],
            Steps:
            [
                new ScriptStepNode(new InterpolatedStringNode("echo hello"), [], span)
            ],
            UnknownFields:
            [
                new UnknownFieldNode(
                    new UnknownScalarNode("meta", span),
                    new UnknownScalarNode("${{ parameters.owner }}", span),
                    span
                )
            ],
            Span: span
        );
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("pipeline.yml"));
    }

    private sealed class FakeStepNodeParameterBinder : IStepNodeParameterBinder
    {
        public bool CanBind(StepNode step)
        {
            return step is ScriptStepNode;
        }

        public StepNode Bind(StepNode step, ParameterBindingContext context)
        {
            return step;
        }
    }
}
