using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Binding;

public interface IPipelineParameterBinder
{
    public PipelineNode Bind(
        PipelineNode pipeline,
        IReadOnlyDictionary<string, string>? parameterOverrides = null);
}
