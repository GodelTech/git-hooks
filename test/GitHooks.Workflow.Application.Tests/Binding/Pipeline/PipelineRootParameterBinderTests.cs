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

namespace GitHooks.Workflow.Application.Tests.Binding.Pipeline;

public sealed class PipelineRootParameterBinderTests
{
    [Fact]
    public void Bind_WithPipeline_RebindsStepsAndUnknownFields()
    {
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var scriptStep = new ScriptStepNode(new InterpolatedStringNode("echo hello"), [], span)
        {
            DisplayName = "original"
        };

        var pipeline = new PipelineNode(
            Parameters: [],
            Steps: [scriptStep],
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

        var binder = CreateBinder(
            bindStep: step => ((ScriptStepNode)step) with
            {
                DisplayName = "bound"
            }
        );

        var result = binder.Bind(pipeline, context);

        var boundStep = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));
        Assert.Equal("bound", boundStep.DisplayName);

        var unknownField = Assert.Single(result.UnknownFields);
        var unknownValue = Assert.IsType<UnknownScalarNode>(unknownField.Value);
        Assert.Equal("contoso", unknownValue.Value);
    }

    [Fact]
    public void Bind_WithUnsupportedUnknownExpression_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var scriptStep = new ScriptStepNode(new InterpolatedStringNode("echo hello"), [], span);

        var pipeline = new PipelineNode(
            Parameters: [],
            Steps: [scriptStep],
            UnknownFields:
            [
                new UnknownFieldNode(
                    new UnknownScalarNode("meta", span),
                    new UnknownScalarNode("${{ variables.owner }}", span),
                    span
                )
            ],
            Span: span
        );

        var binder = CreateBinder(bindStep: step => step);

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind(pipeline, context));

        Assert.Contains("Unsupported expression 'variables.owner'", exception.Message, StringComparison.Ordinal);
    }

    private static PipelineRootParameterBinder CreateBinder(Func<StepNode, StepNode> bindStep)
    {
        var stepBinder = new FakeStepNodeParameterBinder(bindStep);
        var stepsBinder = new StepsParameterBinder(new StepParameterBinder([stepBinder]));
        var unknownBinder = new UnknownNodeParameterBinder(new StringParameterBinder());

        return new PipelineRootParameterBinder(stepsBinder, unknownBinder);
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

    private sealed class FakeStepNodeParameterBinder(Func<StepNode, StepNode> bindStep)
        : IStepNodeParameterBinder
    {
        private readonly Func<StepNode, StepNode> _bindStep = bindStep;

        public bool CanBind(StepNode step)
        {
            return step is ScriptStepNode;
        }

        public StepNode Bind(StepNode step, ParameterBindingContext context)
        {
            return _bindStep(step);
        }
    }
}
