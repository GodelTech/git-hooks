using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Expansion.Source;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion.Source;

internal sealed class LocalPipelineTemplateSourceResolver : IPipelineTemplateSourceResolver
{
    public bool CanResolve(PipelineSource currentSource, TemplateStepNode templateStep)
    {
        return !templateStep.Template.Contains('@');
    }

    /// <inheritdoc/>
    public PipelineSource Resolve(PipelineSource currentSource, TemplateStepNode templateStep)
    {
        ArgumentNullException.ThrowIfNull(templateStep);

        var templateReference = templateStep.Template;
        var span = templateStep.Span;

        if (string.IsNullOrWhiteSpace(templateReference))
        {
            throw new PipelineTemplateExpansionException(
                "Template path cannot be empty.",
                span);
        }

        var sourceDirectory = Path.GetDirectoryName(currentSource.Identifier);

        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new PipelineTemplateExpansionException(
                $"Could not determine directory for source file '{currentSource.Identifier}'.",
                span);
        }

        var resolvedPath = Path.IsPathRooted(templateReference)
            ? templateReference
            : Path.Combine(sourceDirectory, templateReference);

        var fullPath = Path.GetFullPath(resolvedPath);

        return File.Exists(fullPath)
            ? PipelineSource.LocalFile(fullPath)
            : throw new PipelineTemplateExpansionException(
                $"Template file not found: '{fullPath}'.",
                span);
    }
}
