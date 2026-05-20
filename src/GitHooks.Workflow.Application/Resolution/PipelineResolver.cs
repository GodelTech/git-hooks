using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Resolution;

public sealed class PipelineResolver(
    IPipelineCompiler pipelineCompiler,
    IPipelineContentReader pipelineContentReader,
    IPipelineTemplateExpander pipelineTemplateExpander)
    : IPipelineResolver
{
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
    private readonly IPipelineContentReader _pipelineContentReader = pipelineContentReader;
    private readonly IPipelineTemplateExpander _pipelineTemplateExpander = pipelineTemplateExpander;

    /// <inheritdoc/>
    public async Task<PipelineNode> ResolveAsync(
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default)
    {
        var pipelineContent = await _pipelineContentReader.ReadAsync(source, cancellationToken);
        var compiledPipeline = _pipelineCompiler.Compile(pipelineContent, source, parameterOverrides);

        return await _pipelineTemplateExpander.ExpandAsync(compiledPipeline, source, cancellationToken);
    }
}
