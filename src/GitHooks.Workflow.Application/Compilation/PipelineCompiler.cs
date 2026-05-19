using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Parsing;

namespace GitHooks.Workflow.Application.Compilation;

internal sealed class PipelineCompiler(
    IPipelineParser pipelineParser,
    IPipelineParameterBinder pipelineParameterBinder)
    : IPipelineCompiler
{
    private readonly IPipelineParser _pipelineParser = pipelineParser;
    private readonly IPipelineParameterBinder _pipelineParameterBinder = pipelineParameterBinder;

    /// <inheritdoc/>
    public PipelineNode Compile(
        string content,
        string sourceName,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        var pipeline = _pipelineParser.Parse(content, sourceName);

        return _pipelineParameterBinder.Bind(pipeline, parameterOverrides);
    }
}
