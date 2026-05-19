using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion.Exceptions;

namespace GitHooks.Workflow.Application.Expansion;

internal sealed class TemplateExpansionOrchestrator : ITemplateExpansionOrchestrator
{
    /// <inheritdoc/>
    public Task<PipelineNode> OrchestrateAsync(
        PipelineNode pipeline,
        string startingFilePath,
        Func<TemplateStepNode, string, IReadOnlyList<string>, CancellationToken, Task<ResolvedTemplate>> resolveTemplateAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(resolveTemplateAsync);

        var includeChain = new List<string> { startingFilePath };
        var activePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { startingFilePath };

        return ExpandRecursiveAsync(pipeline, startingFilePath, includeChain, activePaths, resolveTemplateAsync, cancellationToken);
    }

    private static async Task<PipelineNode> ExpandRecursiveAsync(
        PipelineNode pipeline,
        string currentFilePath,
        IReadOnlyList<string> includeChain,
        HashSet<string> activePaths,
        Func<TemplateStepNode, string, IReadOnlyList<string>, CancellationToken, Task<ResolvedTemplate>> resolveTemplateAsync,
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

            var resolvedTemplate = await resolveTemplateAsync(templateStep, currentFilePath, includeChain, cancellationToken);
            var nextIncludeChain = includeChain.Append(resolvedTemplate.TemplatePath).ToList();

            if (activePaths.Contains(resolvedTemplate.TemplatePath))
            {
                throw new PipelineTemplateExpansionException(
                    $"Template include cycle detected at '{resolvedTemplate.TemplatePath}'.",
                    templateStep.Span,
                    nextIncludeChain);
            }

            _ = activePaths.Add(resolvedTemplate.TemplatePath);

            try
            {
                var expanded = await ExpandRecursiveAsync(
                    resolvedTemplate.Pipeline,
                    resolvedTemplate.TemplatePath,
                    nextIncludeChain,
                    activePaths,
                    resolveTemplateAsync,
                    cancellationToken);

                expandedSteps.AddRange(expanded.Steps);
            }
            finally
            {
                _ = activePaths.Remove(resolvedTemplate.TemplatePath);
            }
        }

        return pipeline with { Steps = expandedSteps };
    }
}
