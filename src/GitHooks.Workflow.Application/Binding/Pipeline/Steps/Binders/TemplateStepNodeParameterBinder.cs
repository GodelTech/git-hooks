using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Expressions;
using GitHooks.Workflow.Application.Binding.Unknown;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps.Binders;

internal sealed class TemplateStepNodeParameterBinder(
    InterpolationParameterBinder interpolationParameterBinder,
    StringParameterBinder stringParameterBinder,
    UnknownNodeParameterBinder unknownNodeParameterBinder)
    : IStepNodeParameterBinder
{
    private readonly InterpolationParameterBinder _interpolationParameterBinder = interpolationParameterBinder;
    private readonly StringParameterBinder _stringParameterBinder = stringParameterBinder;
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
            Template = _stringParameterBinder.Bind(templateStep.Template, templateStep.Span, context),
            Parameters = templateStep.Parameters.ToDictionary(
                pair => pair.Key,
                pair => _interpolationParameterBinder.Bind(pair.Value, templateStep.Span, context),
                StringComparer.Ordinal
            ),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(templateStep.UnknownFields, context)
        };
    }
}
