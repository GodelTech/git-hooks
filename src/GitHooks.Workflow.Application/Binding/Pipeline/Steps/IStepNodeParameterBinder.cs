using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps;

internal interface IStepNodeParameterBinder
{
    public bool CanBind(StepNode step);

    public StepNode Bind(StepNode step, ParameterBindingContext context);
}
