using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Resolution;

namespace GitHooks.Workflow.Infrastructure.Resolution;

internal sealed class PipelineResolver(
    IPipelineCompiler pipelineCompiler,
    IPipelineTemplateExpander pipelineTemplateExpander)
    : IPipelineResolver
{
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
    private readonly IPipelineTemplateExpander _pipelineTemplateExpander = pipelineTemplateExpander;

    /// <inheritdoc/>
    public async Task<PipelineNode> ResolveAsync(
        string filePath,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fullPath = Path.GetFullPath(filePath);
        var pipelineContent = await File.ReadAllTextAsync(fullPath, cancellationToken);
        var compiledPipeline = _pipelineCompiler.Compile(pipelineContent, fullPath, parameterOverrides);

        return await _pipelineTemplateExpander.ExpandAsync(compiledPipeline, fullPath, cancellationToken);
    }
}
