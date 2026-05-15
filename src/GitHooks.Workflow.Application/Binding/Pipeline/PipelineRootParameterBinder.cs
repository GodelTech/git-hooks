using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Pipeline.Steps;

namespace GitHooks.Workflow.Application.Binding.Pipeline;

internal sealed class PipelineRootParameterBinder(
    StepsParameterBinder stepsParameterBinder,
    UnknownNodeParameterBinder unknownNodeParameterBinder)
{
    private readonly StepsParameterBinder _stepsParameterBinder = stepsParameterBinder;
    private readonly UnknownNodeParameterBinder _unknownNodeParameterBinder = unknownNodeParameterBinder;

    public PipelineNode Bind(PipelineNode pipeline, ParameterBindingContext context)
    {
        return pipeline with
        {
            Steps = _stepsParameterBinder.Bind(pipeline.Steps, context),
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(pipeline.UnknownFields, context)
        };
    }
}
