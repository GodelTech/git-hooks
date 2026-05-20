using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion;

/// <summary>
/// Resolves a template reference relative to the current pipeline source.
/// </summary>
public interface IPipelineTemplateSourceResolver
{
    /// <summary>
    /// Resolves and validates a template source declared in a template step, relative to the provided source.
    /// </summary>
    /// <param name="source">Source that contains the template reference.</param>
    /// <param name="templateStep">Template step that declares the template reference and source span.</param>
    /// <returns>The resolved template source.</returns>
    public PipelineSource Resolve(PipelineSource source, TemplateStepNode templateStep);
}
