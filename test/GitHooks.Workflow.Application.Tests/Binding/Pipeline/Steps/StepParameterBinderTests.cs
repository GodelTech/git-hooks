using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Pipeline.Steps;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Pipeline.Steps;

public sealed class StepParameterBinderTests
{
    [Fact]
    public void Bind_WithSingleApplicableBinder_UsesMatchingBinderAndReturnsBoundStep()
    {
        var span = CreateSpan();
        var step = CreateScriptStep(span);
        var context = CreateContext();

        var nonMatchingBinder = new FakeStepNodeParameterBinder(
            canBind: false,
            bind: (_, _) => throw new InvalidOperationException("Non-matching binder should not be invoked.")
        );

        var expectedStep = CreateScriptStep(span) with
        {
            DisplayName = "bound"
        };

        var matchingBinder = new FakeStepNodeParameterBinder(
            canBind: true,
            bind: (_, _) => expectedStep
        );

        var binder = new StepParameterBinder([nonMatchingBinder, matchingBinder]);

        var result = binder.Bind(step, context);

        Assert.Same(expectedStep, result);
        Assert.False(nonMatchingBinder.BindCalled);
        Assert.True(matchingBinder.BindCalled);
    }

    [Fact]
    public void Bind_WithNoApplicableBinder_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var step = CreateScriptStep(span);
        var context = CreateContext();

        var nonMatchingBinder = new FakeStepNodeParameterBinder(
            canBind: false,
            bind: (_, _) => step
        );

        var binder = new StepParameterBinder([nonMatchingBinder]);

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind(step, context));

        Assert.Contains("No binder available for step node type 'ScriptStepNode'.", exception.Message, StringComparison.Ordinal);
        Assert.False(nonMatchingBinder.BindCalled);
    }

    [Fact]
    public void Bind_WithMultipleApplicableBinders_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var step = CreateScriptStep(span);
        var context = CreateContext();

        var firstMatchingBinder = new FakeStepNodeParameterBinder(
            canBind: true,
            bind: (_, _) => step
        );

        var secondMatchingBinder = new FakeStepNodeParameterBinder(
            canBind: true,
            bind: (_, _) => step
        );

        var binder = new StepParameterBinder([firstMatchingBinder, secondMatchingBinder]);

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.Bind(step, context));

        Assert.Contains("Multiple binders matched step node type 'ScriptStepNode'.", exception.Message, StringComparison.Ordinal);
        Assert.False(firstMatchingBinder.BindCalled);
        Assert.False(secondMatchingBinder.BindCalled);
    }

    private static ScriptStepNode CreateScriptStep(SourceSpan span)
    {
        return new ScriptStepNode(new InterpolatedStringNode("echo hello"), [], span);
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
        bool canBind,
        Func<StepNode, ParameterBindingContext, StepNode> bind)
        : IStepNodeParameterBinder
    {
        private readonly bool _canBind = canBind;
        private readonly Func<StepNode, ParameterBindingContext, StepNode> _bind = bind;

        public bool BindCalled { get; private set; }

        public bool CanBind(StepNode step)
        {
            return _canBind;
        }

        public StepNode Bind(StepNode step, ParameterBindingContext context)
        {
            BindCalled = true;
            return _bind(step, context);
        }
    }
}
