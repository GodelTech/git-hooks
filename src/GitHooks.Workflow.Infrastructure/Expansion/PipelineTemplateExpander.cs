using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Parsing;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal sealed class PipelineTemplateExpander(
    IPipelineParser pipelineParser,
    IPipelineParameterBinder pipelineParameterBinder,
    ITemplatePathResolver templatePathResolver,
    ITemplateExpansionOrchestrator orchestrator)
    : IPipelineTemplateExpander
{
    private readonly IPipelineParser _pipelineParser = pipelineParser;
    private readonly IPipelineParameterBinder _pipelineParameterBinder = pipelineParameterBinder;
    private readonly ITemplatePathResolver _templatePathResolver = templatePathResolver;
    private readonly ITemplateExpansionOrchestrator _orchestrator = orchestrator;

    public Task<PipelineNode> ExpandAsync(
        PipelineNode pipeline,
        string pipelineFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        if (string.IsNullOrWhiteSpace(pipelineFilePath))
        {
            throw new ArgumentException("Pipeline file path cannot be empty.", nameof(pipelineFilePath));
        }

        var rootFilePath = Path.GetFullPath(pipelineFilePath);

        return _orchestrator.OrchestrateAsync(
            pipeline,
            rootFilePath,
            LoadAndBindTemplateAsync,
            cancellationToken);
    }

    private async Task<ResolvedTemplate> LoadAndBindTemplateAsync(
        TemplateStepNode templateStep,
        string currentFilePath,
        IReadOnlyList<string> includeChain,
        CancellationToken cancellationToken)
    {
        string templatePath;

        try
        {
            templatePath = _templatePathResolver.Resolve(currentFilePath, templateStep.Template, templateStep.Span);
        }
        catch (PipelineTemplateExpansionException ex) when (ex.IncludeChain.Count == 0)
        {
            throw new PipelineTemplateExpansionException(ex.Message, ex.Span, includeChain, ex);
        }

        var nextIncludeChain = includeChain.Append(templatePath).ToList();

        var parsedTemplate = await ReadAndParseTemplateAsync(templatePath, templateStep, nextIncludeChain, cancellationToken);
        var boundTemplate = BindTemplateParameters(parsedTemplate, templateStep, nextIncludeChain);

        return new ResolvedTemplate(templatePath, boundTemplate);
    }

    private async Task<PipelineNode> ReadAndParseTemplateAsync(
        string templatePath,
        TemplateStepNode templateStep,
        IReadOnlyList<string> includeChain,
        CancellationToken cancellationToken)
    {
        string templateContent;

        try
        {
            templateContent = await File.ReadAllTextAsync(templatePath, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PipelineTemplateExpansionException(
                $"Failed to read template file '{templatePath}': {ex.Message}",
                templateStep.Span,
                includeChain,
                ex);
        }

        try
        {
            return _pipelineParser.Parse(templateContent, templatePath);
        }
        catch (Exception ex)
        {
            throw new PipelineTemplateExpansionException(
                $"Failed to parse template file '{templatePath}': {ex.Message}",
                templateStep.Span,
                includeChain,
                ex);
        }
    }

    private PipelineNode BindTemplateParameters(
        PipelineNode parsedTemplate,
        TemplateStepNode templateStep,
        IReadOnlyList<string> includeChain)
    {
        Dictionary<string, string>? parameterOverrides = null;

        if (templateStep.Parameters.Count > 0)
        {
            parameterOverrides = templateStep.Parameters.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Value,
                StringComparer.Ordinal);
        }

        try
        {
            return _pipelineParameterBinder.Bind(parsedTemplate, parameterOverrides);
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
