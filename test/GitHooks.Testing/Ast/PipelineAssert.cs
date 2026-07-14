using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class PipelineAssert
{
    public static PipelineNode IsPipeline(
        PipelineNode pipeline,
        int parameterCount,
        int stepCount,
        SourceSpan span)
    {
        Assert.NotNull(pipeline);

        Assert.Equal(
            parameterCount,
            pipeline.Parameters.Count);

        Assert.Equal(
            stepCount,
            pipeline.Steps.Count);

        return IsValidPipeline(
            pipeline,
            span);
    }

    public static ScriptStepNode SingleScriptStep(
        PipelineNode pipeline,
        SourceSpan span)
    {
        Assert.NotNull(pipeline);

        var step = Assert.Single(pipeline.Steps);

        return StepAssert.IsScriptStep(
            step,
            span);
    }

    private static PipelineNode IsValidPipeline(
        PipelineNode pipeline,
        SourceSpan span)
    {
        return PipelineNodeBaseAssert.IsValidPipelineNodeBase<PipelineNode>(
            pipeline,
            span);
    }
}
