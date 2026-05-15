using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.UnknownNodes;

namespace GitHooks.Workflow.Application.Binding.Steps;

internal sealed class TemplateStepParameterBinder(UnknownNodeParameterBinder unknownNodeParameterBinder)
{
    private readonly UnknownNodeParameterBinder _unknownNodeParameterBinder = unknownNodeParameterBinder;

    public TemplateStepNode Bind(TemplateStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Parameters = ParameterBindingHelper.BindInterpolatedStringMap(step.Parameters, context),
            Template = ParameterScalarBinder.Bind(step.Template, step.Span, context),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(step.UnknownFields, context)
        };
    }
}
