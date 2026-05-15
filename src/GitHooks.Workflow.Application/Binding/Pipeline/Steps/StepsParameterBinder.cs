using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps;

internal sealed class StepsParameterBinder(StepParameterBinder stepParameterBinder)
{
    private readonly StepParameterBinder _stepParameterBinder = stepParameterBinder;

    public IReadOnlyList<StepNode> Bind(IReadOnlyList<StepNode> steps, ParameterBindingContext context)
    {
        return [.. steps.Select(step => _stepParameterBinder.Bind(step, context))];
    }
}
