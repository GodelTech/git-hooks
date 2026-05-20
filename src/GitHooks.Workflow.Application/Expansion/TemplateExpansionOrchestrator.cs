using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion;

internal sealed class TemplateExpansionOrchestrator : ITemplateExpansionOrchestrator
{
    /// <inheritdoc/>
    public Task<PipelineNode> OrchestrateAsync(
        PipelineNode pipeline,
        PipelineSource startingSource,
        Func<TemplateStepNode, PipelineSource, IReadOnlyList<PipelineSource>, CancellationToken, Task<ResolvedTemplate>> resolveTemplateAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(resolveTemplateAsync);

        var includeChain = new List<PipelineSource> { startingSource };
        var activeSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { startingSource.Identifier };

        return ExpandRecursiveAsync(pipeline, startingSource, includeChain, activeSourceIds, resolveTemplateAsync, cancellationToken);
    }

    private static async Task<PipelineNode> ExpandRecursiveAsync(
        PipelineNode pipeline,
        PipelineSource currentSource,
        IReadOnlyList<PipelineSource> includeChain,
        HashSet<string> activeSourceIds,
        Func<TemplateStepNode, PipelineSource, IReadOnlyList<PipelineSource>, CancellationToken, Task<ResolvedTemplate>> resolveTemplateAsync,
        CancellationToken cancellationToken)
    {
        var expandedSteps = new List<StepNode>();

        foreach (var step in pipeline.Steps)
        {
            if (step is not TemplateStepNode templateStep)
            {
                expandedSteps.Add(step);
                continue;
            }

            var resolvedTemplate = await resolveTemplateAsync(templateStep, currentSource, includeChain, cancellationToken);
            var nextIncludeChain = includeChain.Append(resolvedTemplate.TemplateSource).ToList();
            var templateSourceId = resolvedTemplate.TemplateSource.Identifier;

            if (activeSourceIds.Contains(templateSourceId))
            {
                throw new PipelineTemplateExpansionException(
                    $"Template include cycle detected at '{templateSourceId}'.",
                    templateStep.Span,
                    nextIncludeChain);
            }

            _ = activeSourceIds.Add(templateSourceId);

            try
            {
                var expanded = await ExpandRecursiveAsync(
                    resolvedTemplate.Pipeline,
                    resolvedTemplate.TemplateSource,
                    nextIncludeChain,
                    activeSourceIds,
                    resolveTemplateAsync,
                    cancellationToken);

                expandedSteps.AddRange(expanded.Steps);
            }
            finally
            {
                _ = activeSourceIds.Remove(templateSourceId);
            }
        }

        return pipeline with { Steps = expandedSteps };
    }
}
