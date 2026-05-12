using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.Steps;
using GitHooks.Workflow.Application.Binding.UnknownNodes;

namespace GitHooks.Workflow.Application.Binding;

/// <summary>
/// Applies Azure DevOps-style <c>${{ parameters.name }}</c> substitutions to a parsed pipeline AST.
/// </summary>
internal sealed class PipelineParameterBinder(
    StepParameterBinder stepParameterBinder,
    UnknownNodeParameterBinder unknownNodeParameterBinder)
    : IPipelineParameterBinder
{
    private readonly StepParameterBinder _stepParameterBinder = stepParameterBinder;
    private readonly UnknownNodeParameterBinder _unknownNodeParameterBinder = unknownNodeParameterBinder;

    /// <inheritdoc/>
    public PipelineNode Bind(PipelineNode pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        var context = ParameterBindingContext.Create(pipeline.Parameters);

        return pipeline with
        {
            Steps = [.. pipeline.Steps.Select(step => _stepParameterBinder.Bind(step, context))],
            UnknownFields = _unknownNodeParameterBinder.BindUnknownFields(pipeline.UnknownFields, context)
        };
    }
}
