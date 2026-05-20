using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

/// <summary>
/// Resolves a template path relative to a local-file pipeline source.
/// </summary>
internal sealed class LocalPipelineTemplateSourceResolver : IPipelineTemplateSourceResolverStrategy
{
    public bool CanResolve(PipelineSource source)
    {
        return source.Kind is PipelineSourceKind.LocalFile;
    }

    /// <inheritdoc/>
    public PipelineSource Resolve(PipelineSource source, TemplateStepNode templateStep)
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

        var sourceDirectory = Path.GetDirectoryName(source.Identifier);

        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new PipelineTemplateExpansionException(
                $"Could not determine directory for source file '{source.Identifier}'.",
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
