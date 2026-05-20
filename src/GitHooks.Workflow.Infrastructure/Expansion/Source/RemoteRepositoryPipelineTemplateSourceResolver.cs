using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Expansion.Source;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion.Source;

internal sealed class RemoteRepositoryPipelineTemplateSourceResolver : IPipelineTemplateSourceResolver
{
    public bool CanResolve(PipelineSource currentSource, TemplateStepNode templateStep)
    {
        return templateStep.Template.Contains('@');
    }

    public PipelineSource Resolve(PipelineSource currentSource, TemplateStepNode templateStep)
    {
        ArgumentNullException.ThrowIfNull(templateStep);

        // todo: implement remote repository template resolution
        throw new PipelineTemplateExpansionException(
            "Remote repository template resolution is not implemented yet.",
            templateStep.Span,
            [currentSource]);
    }
}
