using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion.Source;

public interface IPipelineTemplateSourceResolver
{
    public bool CanResolve(PipelineSource currentSource, TemplateStepNode templateStep);

    public PipelineSource Resolve(PipelineSource currentSource, TemplateStepNode templateStep);
}
