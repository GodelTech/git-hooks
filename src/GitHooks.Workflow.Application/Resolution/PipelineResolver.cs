using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Resolution;

public sealed class PipelineResolver(
    IPipelineCompiler pipelineCompiler,
    IPipelineTemplateExpander pipelineTemplateExpander)
    : IPipelineResolver
{
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
    private readonly IPipelineTemplateExpander _pipelineTemplateExpander = pipelineTemplateExpander;

    /// <inheritdoc/>
    public async Task<PipelineNode> ResolveAsync(
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default)
    {
        var compiledPipeline = await _pipelineCompiler.CompileAsync(source, parameterOverrides, cancellationToken);

        return await _pipelineTemplateExpander.ExpandAsync(compiledPipeline, source, cancellationToken);
    }
}
