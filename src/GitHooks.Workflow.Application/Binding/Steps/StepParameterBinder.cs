using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.UnknownNodes;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Steps;

internal sealed class StepParameterBinder(UnknownNodeParameterBinder unknownNodeParameterBinder)
{
    private readonly UnknownNodeParameterBinder _unknownNodeParameterBinder = unknownNodeParameterBinder;

    public StepNode Bind(StepNode step, ParameterBindingContext context)
    {
        return step switch
        {
            ScriptStepNode scriptStep => BindScriptStep(scriptStep, context),
            TemplateStepNode templateStep => BindTemplateStep(templateStep, context),
            _ => throw new PipelineParameterBindingException(
                $"Unsupported step node type '{step.GetType().Name}'.",
                step.Span
            )
        };
    }

    private ScriptStepNode BindScriptStep(ScriptStepNode step, ParameterBindingContext context)
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
            Env = BindInterpolatedStringMap(step.Env, context),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(step.UnknownFields, context)
        };
    }

    private TemplateStepNode BindTemplateStep(TemplateStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Parameters = BindInterpolatedStringMap(step.Parameters, context),
            Template = ParameterScalarBinder.Bind(step.Template, step.Span, context),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(step.UnknownFields, context)
        };
    }

    private static Dictionary<string, InterpolatedStringNode> BindInterpolatedStringMap(
        IReadOnlyDictionary<string, InterpolatedStringNode> values,
        ParameterBindingContext context)
    {
        return values.ToDictionary(
            pair => pair.Key,
            pair => new InterpolatedStringNode(
                ParameterScalarBinder.Bind(pair.Value.Value, SourceSpan.Unknown(new SourceRef(pair.Key)), context)
            ),
            StringComparer.Ordinal
        );
    }
}
