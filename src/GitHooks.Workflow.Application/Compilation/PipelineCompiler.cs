using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Domain.Model;

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
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var pipeline = _pipelineParser.Parse(content, source.Identifier);

        return _pipelineParameterBinder.Bind(pipeline, parameterOverrides);
    }
}
