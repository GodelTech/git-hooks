using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

internal interface IStepNodeBuilder
{
    public bool CanBuild(StepFields fields);

    public StepNode Build(StepFields fields, SourceSpan span);
}
