using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Compilation;

internal sealed class PipelineCompiler(
    IPipelineContentReader pipelineContentReader,
    IPipelineParser pipelineParser,
    IPipelineParameterBinder pipelineParameterBinder)
    : IPipelineCompiler
{
    private readonly IPipelineContentReader _pipelineContentReader = pipelineContentReader;
    private readonly IPipelineParser _pipelineParser = pipelineParser;
    private readonly IPipelineParameterBinder _pipelineParameterBinder = pipelineParameterBinder;

    public async Task<PipelineNode> CompileAsync(
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null,
        CancellationToken cancellationToken = default)
    {
        var content = await _pipelineContentReader.ReadAsync(source, cancellationToken);
        var pipeline = _pipelineParser.Parse(content, source.Identifier);

        return _pipelineParameterBinder.Bind(pipeline, parameterOverrides);
    }
}
