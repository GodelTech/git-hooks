using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Expressions;

namespace GitHooks.Workflow.Application.Binding.Pipeline.Steps.Binders;

internal sealed class ScriptStepNodeParameterBinder(
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
                : _stringBinder.Bind(scriptStep.DisplayName, scriptStep.Span, context),
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
