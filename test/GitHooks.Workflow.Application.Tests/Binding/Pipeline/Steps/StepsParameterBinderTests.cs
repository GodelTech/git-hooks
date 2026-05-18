using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Pipeline.Steps;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Pipeline.Steps;

public sealed class StepsParameterBinderTests
{
    [Fact]
    public void Bind_WithMultipleSteps_BindsAllStepsInOrder()
    {
        var span = CreateSpan();
        var context = CreateContext();

        var firstStep = new ScriptStepNode(new InterpolatedStringNode("echo first"), [], span)
        {
            DisplayName = "first"
        };

        var secondStep = new ScriptStepNode(new InterpolatedStringNode("echo second"), [], span)
        {
            DisplayName = "second"
        };

        var applicableBinder = new FakeStepNodeParameterBinder(
            canBind: _ => true,
            bind: (step, _) => ((ScriptStepNode)step) with
            {
                DisplayName = $"bound-{((ScriptStepNode)step).DisplayName}"
            }
        );

        var stepParameterBinder = new StepParameterBinder([applicableBinder]);
        var binder = new StepsParameterBinder(stepParameterBinder);

        var result = binder.Bind([firstStep, secondStep], context);

        Assert.Equal(2, result.Count);

        var boundFirst = Assert.IsType<ScriptStepNode>(result[0]);
        Assert.Equal("bound-first", boundFirst.DisplayName);

        var boundSecond = Assert.IsType<ScriptStepNode>(result[1]);
        Assert.Equal("bound-second", boundSecond.DisplayName);

        Assert.Equal(2, applicableBinder.BindCallCount);
    }

    [Fact]
    public void Bind_WhenAnyStepHasNoApplicableBinder_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var context = CreateContext();

        var scriptStep = new ScriptStepNode(new InterpolatedStringNode("echo first"), [], span);

        var templateStep = new TemplateStepNode(
            Template: "template.yml",
            Parameters: new Dictionary<string, InterpolatedStringNode>(),
            UnknownFields: [],
            Span: span
        );

        var scriptOnlyBinder = new FakeStepNodeParameterBinder(
            canBind: step => step is ScriptStepNode,
            bind: (step, _) => step
        );

        var stepParameterBinder = new StepParameterBinder([scriptOnlyBinder]);
        var binder = new StepsParameterBinder(stepParameterBinder);

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind([scriptStep, templateStep], context));

        Assert.Contains("No binder available for step node type 'TemplateStepNode'.", exception.Message, StringComparison.Ordinal);
        Assert.Equal(1, scriptOnlyBinder.BindCallCount);
    }

    private static ParameterBindingContext CreateContext()
    {
        return ParameterBindingContext.Create([]);
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("pipeline.yml"));
    }

    private sealed class FakeStepNodeParameterBinder(
        Func<StepNode, bool> canBind,
        Func<StepNode, ParameterBindingContext, StepNode> bind)
        : IStepNodeParameterBinder
    {
        private readonly Func<StepNode, bool> _canBind = canBind;
        private readonly Func<StepNode, ParameterBindingContext, StepNode> _bind = bind;

        public int BindCallCount { get; private set; }

        public bool CanBind(StepNode step)
        {
            return _canBind(step);
        }

        public StepNode Bind(StepNode step, ParameterBindingContext context)
        {
            BindCallCount++;
            return _bind(step, context);
        }
    }
}
