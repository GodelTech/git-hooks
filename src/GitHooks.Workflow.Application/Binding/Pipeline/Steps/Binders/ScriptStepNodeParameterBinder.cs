using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Expressions;
using GitHooks.Workflow.Application.Binding.Unknown;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps.Binders;

internal sealed class ScriptStepNodeParameterBinder(
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
        return step is ScriptStepNode;
    }

    public StepNode Bind(StepNode step, ParameterBindingContext context)
    {
        var scriptStep = (ScriptStepNode)step;

        return scriptStep with
        {
            Script = _interpolationParameterBinder.Bind(scriptStep.Script, scriptStep.Span, context),
            DisplayName = scriptStep.DisplayName is null
                ? null
                : _stringParameterBinder.Bind(scriptStep.DisplayName, scriptStep.Span, context),
            WorkingDirectory = scriptStep.WorkingDirectory is null
                ? null
                : _interpolationParameterBinder.Bind(scriptStep.WorkingDirectory, scriptStep.Span, context),
            Env = scriptStep.Env.ToDictionary(
                pair => pair.Key,
                pair => _interpolationParameterBinder.Bind(pair.Value, scriptStep.Span, context),
                StringComparer.Ordinal
            ),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(scriptStep.UnknownFields, context)
        };
    }
}
