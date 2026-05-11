using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.UnknownNodes;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Steps;

internal sealed class StepParameterBinder
{
    public static StepNode Bind(StepNode step, ParameterBindingContext context)
    {
        var boundStep = step switch
        {
            ScriptStepNode scriptStep => BindScriptStep(scriptStep, context),
            TemplateStepNode templateStep => BindTemplateStep(templateStep, context),
            _ => throw new PipelineParameterBindingException(
                $"Unsupported step node type '{step.GetType().Name}'.",
                step.Span
            )
        };

        return boundStep with
        {
            DisplayName = boundStep.DisplayName is null
                ? null
                : ParameterScalarBinder.Bind(boundStep.DisplayName, boundStep.Span, context),
            WorkingDirectory = boundStep.WorkingDirectory is null
                ? null
                : new InterpolatedStringNode(ParameterScalarBinder.Bind(boundStep.WorkingDirectory.Value, boundStep.Span, context)),
            Env = BindInterpolatedStringMap(boundStep.Env, context),
            UnknownFields = UnknownNodeParameterBinder.BindUnknownFields(boundStep.UnknownFields, context)
        };
    }

    private static StepNode BindScriptStep(ScriptStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Script = new InterpolatedStringNode(ParameterScalarBinder.Bind(step.Script.Value, step.Span, context))
        };
    }

    private static StepNode BindTemplateStep(TemplateStepNode step, ParameterBindingContext context)
    {
        return step with
        {
            Parameters = BindInterpolatedStringMap(step.Parameters, context),
            Template = ParameterScalarBinder.Bind(step.Template, step.Span, context),
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

