using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal sealed class PipelineTemplateExpander(
    IPipelineParser pipelineParser,
    IPipelineParameterBinder pipelineParameterBinder)
    : IPipelineTemplateExpander
{
    private readonly IPipelineParser _pipelineParser = pipelineParser;
    private readonly IPipelineParameterBinder _pipelineParameterBinder = pipelineParameterBinder;

    public async Task<PipelineNode> ExpandAsync(
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
        var includeChain = new List<string> { rootFilePath };
        var activePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFilePath };

        return await ExpandPipelineInternalAsync(
            pipeline,
            rootFilePath,
            includeChain,
            activePaths,
            cancellationToken);
    }

    private async Task<PipelineNode> ExpandPipelineInternalAsync(
        PipelineNode pipeline,
        string currentFilePath,
        IReadOnlyList<string> includeChain,
        HashSet<string> activePaths,
        CancellationToken cancellationToken)
    {
        static string ResolveTemplatePath(string sourceFilePath, string templatePath, SourceSpan span)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                throw new PipelineTemplateExpansionException(
                    "Template path cannot be empty.",
                    span);
            }

            var sourceDirectory = Path.GetDirectoryName(sourceFilePath);

            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new PipelineTemplateExpansionException(
                    $"Could not determine directory for source file '{sourceFilePath}'.",
                    span);
            }

            var resolvedPath = Path.IsPathRooted(templatePath)
                ? templatePath
                : Path.Combine(sourceDirectory, templatePath);

            var fullPath = Path.GetFullPath(resolvedPath);

            if (!File.Exists(fullPath))
            {
                throw new PipelineTemplateExpansionException(
                    $"Template file not found: '{fullPath}'.",
                    span);
            }

            return fullPath;
        }

        var expandedSteps = new List<StepNode>();

        foreach (var step in pipeline.Steps)
        {
            if (step is not TemplateStepNode templateStep)
            {
                expandedSteps.Add(step);
                continue;
            }

            var templatePath = ResolveTemplatePath(currentFilePath, templateStep.Template, templateStep.Span);
            var nextIncludeChain = includeChain.Append(templatePath).ToList();

            if (activePaths.Contains(templatePath))
            {
                throw new PipelineTemplateExpansionException(
                    $"Template include cycle detected at '{templatePath}'.",
                    templateStep.Span,
                    nextIncludeChain);
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
                    nextIncludeChain,
                    ex);
            }

            PipelineNode parsedTemplate;

            try
            {
                parsedTemplate = _pipelineParser.Parse(templateContent, templatePath);
            }
            catch (Exception ex)
            {
                throw new PipelineTemplateExpansionException(
                    $"Failed to parse template file '{templatePath}': {ex.Message}",
                    templateStep.Span,
                    nextIncludeChain,
                    ex);
            }

            Dictionary<string, string>? templateParameterOverrides = null;

            if (templateStep.Parameters.Count > 0)
            {
                templateParameterOverrides = templateStep.Parameters.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Value,
                    StringComparer.Ordinal);
            }

            PipelineNode boundTemplate;

            try
            {
                boundTemplate = _pipelineParameterBinder.Bind(parsedTemplate, templateParameterOverrides);
            }
            catch (PipelineParameterBindingException ex)
            {
                throw new PipelineTemplateExpansionException(
                    ex.Message,
                    ex.Span,
                    nextIncludeChain,
                    ex);
            }

            _ = activePaths.Add(templatePath);

            PipelineNode expandedTemplate;

            try
            {
                expandedTemplate = await ExpandPipelineInternalAsync(
                    boundTemplate,
                    templatePath,
                    nextIncludeChain,
                    activePaths,
                    cancellationToken);
            }
            finally
            {
                _ = activePaths.Remove(templatePath);
            }

            expandedSteps.AddRange(expandedTemplate.Steps);
        }

        return pipeline with
        {
            Steps = expandedSteps
        };
    }
}
