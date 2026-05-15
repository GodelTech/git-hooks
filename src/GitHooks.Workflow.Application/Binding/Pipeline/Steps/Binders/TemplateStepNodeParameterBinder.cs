using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Expressions;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps.Binders;

internal sealed class TemplateStepNodeParameterBinder(
    InterpolationParameterBinder interpolationParameterBinder,
    StringBinder stringBinder,
    UnknownNodeParameterBinder unknownNodeParameterBinder)
    : IStepNodeParameterBinder
{
    private readonly InterpolationParameterBinder _interpolationParameterBinder = interpolationParameterBinder;
    private readonly StringBinder _stringBinder = stringBinder;
    private readonly UnknownNodeParameterBinder _unknownNodeParameterBinder = unknownNodeParameterBinder;

    public bool CanBind(StepNode step)
    {
        return step is TemplateStepNode;
    }

    public StepNode Bind(StepNode step, ParameterBindingContext context)
    {
        var templateStep = (TemplateStepNode)step;

        return templateStep with
        {
            Template = _stringBinder.Bind(templateStep.Template, templateStep.Span, context),
            Parameters = templateStep.Parameters.ToDictionary(
                pair => pair.Key,
                pair => _interpolationParameterBinder.Bind(pair.Value, templateStep.Span, context),
                StringComparer.Ordinal
            ),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(templateStep.UnknownFields, context)
        };
    }
}
