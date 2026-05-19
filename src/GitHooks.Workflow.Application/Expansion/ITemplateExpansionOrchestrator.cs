using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Expansion;

/// <summary>
/// Orchestrates recursive template expansion, managing cycle detection and step flattening.
/// Template resolution is delegated to the <paramref name="resolveTemplateAsync"/> callback.
/// </summary>
public interface ITemplateExpansionOrchestrator
{
    /// <summary>
    /// Recursively expands all template steps in <paramref name="pipeline"/> and returns a flat pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline whose template steps should be expanded.</param>
    /// <param name="startingFilePath">Absolute path of the root pipeline file. Used as the initial active path for cycle detection.</param>
    /// <param name="resolveTemplateAsync">
    /// Callback that resolves a template step into a <see cref="ResolvedTemplate"/>.
    /// Receives the template step, the current file path, the current include chain, and a cancellation token.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
    /// <returns>A pipeline with all template steps expanded into their constituent steps.</returns>
    public Task<PipelineNode> OrchestrateAsync(
        PipelineNode pipeline,
        string startingFilePath,
        Func<TemplateStepNode, string, IReadOnlyList<string>, CancellationToken, Task<ResolvedTemplate>> resolveTemplateAsync,
        CancellationToken cancellationToken = default);
}
