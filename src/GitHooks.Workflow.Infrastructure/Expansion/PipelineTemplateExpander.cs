using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal sealed class PipelineTemplateExpander(
    IPipelineCompiler pipelineCompiler,
    IPipelineContentReader pipelineContentReader,
    IPipelineTemplateSourceResolver templateSourceResolver,
    ITemplateExpansionOrchestrator orchestrator)
    : IPipelineTemplateExpander
{
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
    private readonly IPipelineContentReader _pipelineContentReader = pipelineContentReader;
    private readonly IPipelineTemplateSourceResolver _templateSourceResolver = templateSourceResolver;
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
            LoadAndBindTemplateAsync,
            cancellationToken);
    }

    private async Task<ResolvedTemplate> LoadAndBindTemplateAsync(
        TemplateStepNode templateStep,
        PipelineSource currentSource,
        IReadOnlyList<PipelineSource> includeChain,
        CancellationToken cancellationToken)
    {
        PipelineSource templateSource;

        try
        {
            templateSource = _templateSourceResolver.Resolve(currentSource, templateStep);
        }
        catch (PipelineTemplateExpansionException ex) when (ex.IncludeChain.Count == 0)
        {
            throw new PipelineTemplateExpansionException(
                ex.Message,
                ex.Span,
                includeChain,
                ex);
        }

        var nextIncludeChain = includeChain.Append(templateSource).ToList();

        var boundTemplate = await ReadAndCompileTemplateAsync(templateSource, templateStep, nextIncludeChain, cancellationToken);

        return new ResolvedTemplate(templateSource, boundTemplate);
    }

    private async Task<PipelineNode> ReadAndCompileTemplateAsync(
        PipelineSource templateSource,
        TemplateStepNode templateStep,
        IReadOnlyList<PipelineSource> includeChain,
        CancellationToken cancellationToken)
    {
        static Dictionary<string, string>? CreateParameterOverrides(TemplateStepNode currentTemplateStep)
        {
            return currentTemplateStep.Parameters.Count == 0
                ? null
                : currentTemplateStep.Parameters.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Value,
                    StringComparer.Ordinal);
        }

        string templateContent;

        try
        {
            templateContent = await _pipelineContentReader.ReadAsync(templateSource, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PipelineTemplateExpansionException(
                $"Failed to read template file '{templateSource.Identifier}': {ex.Message}",
                templateStep.Span,
                includeChain,
                ex);
        }

        try
        {
            return _pipelineCompiler.Compile(templateContent, templateSource, CreateParameterOverrides(templateStep));
        }
        catch (PipelineParsingException ex)
        {
            throw new PipelineTemplateExpansionException(
                $"Failed to parse template file '{templateSource.Identifier}': {ex.Message}",
                templateStep.Span,
                includeChain,
                ex);
        }
        catch (PipelineParameterBindingException ex)
        {
            throw new PipelineTemplateExpansionException(
                ex.Message,
                ex.Span,
                includeChain,
                ex);
        }
    }
}
