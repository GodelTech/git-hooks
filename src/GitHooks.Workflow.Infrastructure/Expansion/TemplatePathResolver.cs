using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

/// <summary>
/// Resolves a template path relative to the source pipeline file.
/// </summary>
internal sealed class TemplatePathResolver : ITemplatePathResolver
{
    /// <inheritdoc/>
    public string Resolve(string sourceFilePath, string templatePath, SourceSpan span)
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

        return File.Exists(fullPath)
            ? fullPath
            : throw new PipelineTemplateExpansionException(
                $"Template file not found: '{fullPath}'.",
                span);
    }
}
