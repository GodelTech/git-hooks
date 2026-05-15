using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.UnknownNodes;

namespace GitHooks.Workflow.Application.Binding.Steps;

internal sealed class ScriptStepParameterBinder(UnknownNodeParameterBinder unknownNodeParameterBinder)
{
    private readonly UnknownNodeParameterBinder _unknownNodeParameterBinder = unknownNodeParameterBinder;

    public ScriptStepNode Bind(ScriptStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Script = new InterpolatedStringNode(ParameterScalarBinder.Bind(step.Script.Value, step.Span, context)),
            DisplayName = step.DisplayName is null
                ? null
                : ParameterScalarBinder.Bind(step.DisplayName, step.Span, context),
            WorkingDirectory = step.WorkingDirectory is null
                ? null
                : new InterpolatedStringNode(ParameterScalarBinder.Bind(step.WorkingDirectory.Value, step.Span, context)),
            Env = ParameterBindingHelper.BindInterpolatedStringMap(step.Env, context),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(step.UnknownFields, context)
        };
    }
}
