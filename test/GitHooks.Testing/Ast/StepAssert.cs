using System.Diagnostics;

using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Domain.Syntax;

namespace GitHooks.Testing.Ast;

public static class StepAssert
{
    public static ScriptStepNode IsScriptStep(
        StepNode step,
        SourceSpan span)
    {
        var actualStep = IsValidStep<ScriptStepNode>(
            step,
            span);

        return actualStep;
    }

    public static TemplateStepNode IsTemplateStep(
        StepNode step,
        SourceSpan span)
    {
        var actualStep = IsValidStep<TemplateStepNode>(
            step,
            span);

        return actualStep;
    }

    public static InvalidStepNode IsInvalidStep(
        StepNode step,
        SourceSpan span)
    {
        var actualStep = IsValidStep<InvalidStepNode>(
            step,
            span);

        return actualStep;
    }

    public static StringKeyFieldNode<ExpressionNode> GetRequiredField(
        StepNode step,
        string fieldName)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        return fieldName switch
        {
            StepFieldNames.Script
                => Assert.IsType<ScriptStepNode>(step).Script,

            StepFieldNames.Template
                => Assert.IsType<TemplateStepNode>(step).Template,

            StepFieldNames.DisplayName
                => Assert.IsType<ScriptStepNode>(step).DisplayName
                    ?? AssertMissingField(fieldName),

            StepFieldNames.Condition
                => Assert.IsType<ScriptStepNode>(step).Condition
                    ?? AssertMissingField(fieldName),

            StepFieldNames.TimeoutInMinutes
                => Assert.IsType<ScriptStepNode>(step).TimeoutInMinutes
                    ?? AssertMissingField(fieldName),

            StepFieldNames.WorkingDirectory
                => Assert.IsType<ScriptStepNode>(step).WorkingDirectory
                    ?? AssertMissingField(fieldName),

            _ => throw new ArgumentOutOfRangeException(
                nameof(fieldName),
                fieldName,
                "Unknown step field."),
        };
    }

    private static StringKeyFieldNode<ExpressionNode> AssertMissingField(
        string fieldName)
    {
        Assert.Fail($"Expected step field '{fieldName}' to be present.");

        throw new UnreachableException();
    }

    private static TNode IsValidStep<TNode>(
        StepNode step,
        SourceSpan span)
        where TNode : StepNode
    {
        return PipelineNodeBaseAssert.IsValidPipelineNodeBase<TNode>(
            step,
            span);
    }
}
