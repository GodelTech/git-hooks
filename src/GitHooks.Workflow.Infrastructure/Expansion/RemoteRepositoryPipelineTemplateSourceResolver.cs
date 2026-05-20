using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal sealed class RemoteRepositoryPipelineTemplateSourceResolver : IPipelineTemplateSourceResolverStrategy
{
    public bool CanResolve(PipelineSource source)
    {
        return source.Kind is PipelineSourceKind.RemoteRepository;
    }

    public PipelineSource Resolve(PipelineSource source, TemplateStepNode templateStep)
    {
        ArgumentNullException.ThrowIfNull(templateStep);

        throw new PipelineTemplateExpansionException(
            "Remote repository template resolution is not implemented yet.",
            templateStep.Span,
            [source]);
    }
}
