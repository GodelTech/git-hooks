using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion;

public interface ITemplateExpansionOrchestrator
{
    /// <summary>
    /// Recursively expands all template steps in <paramref name="pipeline"/> and returns a flat pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline whose template steps should be expanded.</param>
    /// <param name="startingSource">Root source of the pipeline. Used as the initial active source for cycle detection.</param>
    /// <param name="resolveTemplateAsync">
    /// Callback that resolves a template step into a <see cref="ResolvedTemplate"/>.
    /// Receives the template step, the current source, the current include chain, and a cancellation token.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for asynchronous operations.</param>
    /// <returns>A pipeline with all template steps expanded into their constituent steps.</returns>
    public Task<PipelineNode> OrchestrateAsync(
        PipelineNode pipeline,
        PipelineSource startingSource,
        Func<TemplateStepNode, PipelineSource, IReadOnlyList<PipelineSource>, CancellationToken, Task<ResolvedTemplate>> resolveTemplateAsync,
        CancellationToken cancellationToken = default);
}
