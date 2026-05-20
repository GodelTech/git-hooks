using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal sealed class CompositePipelineTemplateSourceResolver(
    IEnumerable<IPipelineTemplateSourceResolverStrategy> resolvers)
    : IPipelineTemplateSourceResolver
{
    private readonly IReadOnlyList<IPipelineTemplateSourceResolverStrategy> _resolvers = [.. resolvers];

    public PipelineSource Resolve(PipelineSource source, TemplateStepNode templateStep)
    {
        var resolver = _resolvers.FirstOrDefault(candidate => candidate.CanResolve(source));

        return resolver is not null
            ? resolver.Resolve(source, templateStep)
            : throw new PipelineTemplateExpansionException(
                $"No template source resolver registered for source kind '{source.Kind}'.",
                templateStep.Span,
                [source]);
    }
}
