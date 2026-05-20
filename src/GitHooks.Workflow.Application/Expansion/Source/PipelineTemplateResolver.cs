using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion.Source;

public sealed class PipelineTemplateResolver(IEnumerable<IPipelineTemplateSourceResolver> resolvers)
{
    private readonly IReadOnlyList<IPipelineTemplateSourceResolver> _resolvers = [.. resolvers];

    public PipelineSource Resolve(PipelineSource currentSource, TemplateStepNode templateStep)
    {
        ArgumentNullException.ThrowIfNull(templateStep);

        var resolver = _resolvers.FirstOrDefault(resolver => resolver.CanResolve(currentSource, templateStep));

        return resolver is not null
            ? resolver.Resolve(currentSource, templateStep)
            : throw new InvalidOperationException($"No resolver found for the given pipeline source and template step '{templateStep.Template}'.");
    }
}
