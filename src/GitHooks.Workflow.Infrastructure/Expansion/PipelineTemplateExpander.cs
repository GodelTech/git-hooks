using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal sealed class PipelineTemplateExpander(
    IPipelineCompiler pipelineCompiler,
    ITemplatePathResolver templatePathResolver,
    ITemplateExpansionOrchestrator orchestrator)
    : IPipelineTemplateExpander
{
    private readonly IPipelineCompiler _pipelineCompiler = pipelineCompiler;
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

        var boundTemplate = await ReadAndCompileTemplateAsync(templatePath, templateStep, nextIncludeChain, cancellationToken);

        return new ResolvedTemplate(templatePath, boundTemplate);
    }

    private async Task<PipelineNode> ReadAndCompileTemplateAsync(
        string templatePath,
        TemplateStepNode templateStep,
        IReadOnlyList<string> includeChain,
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
            return _pipelineCompiler.Compile(templateContent, templatePath, CreateParameterOverrides(templateStep));
        }
        catch (YamlParseException ex)
        {
            throw new PipelineTemplateExpansionException(
                $"Failed to parse template file '{templatePath}': {ex.Message}",
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
