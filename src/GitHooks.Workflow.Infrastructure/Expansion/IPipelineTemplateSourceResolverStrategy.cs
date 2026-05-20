using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

internal interface IPipelineTemplateSourceResolverStrategy
{
    public bool CanResolve(PipelineSource source);

    public PipelineSource Resolve(PipelineSource source, TemplateStepNode templateStep);
}
