using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Pipeline;

namespace GitHooks.Workflow.Application.Binding;

internal sealed class PipelineParameterBinder(PipelineRootParameterBinder pipelineRootParameterBinder)
    : IPipelineParameterBinder
{
    private readonly PipelineRootParameterBinder _pipelineRootParameterBinder = pipelineRootParameterBinder;

    /// <inheritdoc/>
    public PipelineNode Bind(
        PipelineNode pipeline,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var context = ParameterBindingContext.Create(pipeline.Parameters, parameterOverrides);

        return _pipelineRootParameterBinder.Bind(pipeline, context);
    }
}
