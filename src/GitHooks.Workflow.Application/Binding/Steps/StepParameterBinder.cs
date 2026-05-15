using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Parameters;

namespace GitHooks.Workflow.Application.Binding.Steps;

internal sealed class StepParameterBinder(
    ScriptStepParameterBinder scriptStepBinder,
    TemplateStepParameterBinder templateStepBinder)
{
    private readonly ScriptStepParameterBinder _scriptStepBinder = scriptStepBinder;
    private readonly TemplateStepParameterBinder _templateStepBinder = templateStepBinder;

    public StepNode Bind(StepNode step, ParameterBindingContext context)
    {
        return step switch
        {
            ScriptStepNode scriptStep => _scriptStepBinder.Bind(scriptStep, context),
            TemplateStepNode templateStep => _templateStepBinder.Bind(templateStep, context),
            _ => throw new PipelineParameterBindingException(
                $"Unsupported step node type '{step.GetType().Name}'.",
                step.Span
            )
        };
    }
}
