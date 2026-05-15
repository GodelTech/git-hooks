using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps;

internal sealed class StepParameterBinder(IEnumerable<IStepNodeParameterBinder> binders)
{
    private readonly List<IStepNodeParameterBinder> _binders = [.. binders];

    public StepNode Bind(StepNode step, ParameterBindingContext context)
    {
        var applicableBinders = _binders
            .Where(binder => binder.CanBind(step))
            .ToList();

        return applicableBinders.Count switch
        {
            1 => applicableBinders[0].Bind(step, context),
            0 => throw new PipelineParameterBindingException(
                $"No binder available for step node type '{step.GetType().Name}'.",
                step.Span
            ),
            _ => throw new PipelineParameterBindingException(
                $"Multiple binders matched step node type '{step.GetType().Name}'.",
                step.Span
            )
        };
    }
}
