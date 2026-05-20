using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Compilation;

public interface IPipelineCompiler
{
    public Task<PipelineNode> CompileAsync(
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default);
}
