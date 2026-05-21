using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Expansion.Source;
using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion;

public sealed class PipelineTemplateExpander(
    IPipelineCompiler pipelineCompiler,
    PipelineTemplateResolver templateResolver,
    ITemplateExpansionOrchestrator orchestrator)
    : IPipelineTemplateExpander
{
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
    private readonly PipelineTemplateResolver _templateResolver = templateResolver;
    private readonly ITemplateExpansionOrchestrator _orchestrator = orchestrator;

    public Task<PipelineNode> ExpandAsync(
        PipelineNode pipeline,
        PipelineSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        return _orchestrator.OrchestrateAsync(
            pipeline,
            source,
            ResolveTemplateAsync,
            cancellationToken);
    }

    private async Task<ResolvedTemplate> ResolveTemplateAsync(
        TemplateStepNode templateStep,
        PipelineSource currentSource,
        IReadOnlyList<PipelineSource> includeChain,
        CancellationToken cancellationToken)
    {
        PipelineSource templateSource;

        try
        {
            templateSource = _templateResolver.Resolve(currentSource, templateStep);
        }
        catch (PipelineTemplateExpansionException exception) when (exception.IncludeChain.Count == 0)
        {
            throw new PipelineTemplateExpansionException(
                exception.Message,
                exception.Span,
                includeChain,
                exception);
        }

        IReadOnlyDictionary<string, string>? parameterOverrides = null;

        if (templateStep.Parameters.Count > 0)
        {
            parameterOverrides = templateStep.Parameters.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Value,
                StringComparer.Ordinal);
        }

        var nextIncludeChain = includeChain.Append(templateSource).ToList();
        var compiledPipeline = await CompileTemplateAsync(templateSource, parameterOverrides, nextIncludeChain, cancellationToken);

        return new ResolvedTemplate(templateSource, compiledPipeline);
    }

    private async Task<PipelineNode> CompileTemplateAsync(
        PipelineSource templateSource,
        IReadOnlyDictionary<string, string>? parameterOverrides,
        IReadOnlyList<PipelineSource> includeChain,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _pipelineCompiler.CompileAsync(templateSource, parameterOverrides, cancellationToken);
        }
        catch (PipelineParsingException exception)
        {
            throw new PipelineTemplateExpansionException(
                exception.Message,
                exception.Span,
                includeChain,
                exception);
        }
        catch (PipelineParameterBindingException exception)
        {
            throw new PipelineTemplateExpansionException(
                exception.Message,
                exception.Span,
                includeChain,
                exception);
        }
    }
}
