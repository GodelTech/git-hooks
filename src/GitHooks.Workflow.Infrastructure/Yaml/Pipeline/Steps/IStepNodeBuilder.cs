using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

internal interface IStepNodeBuilder
{
    public bool CanBuild(StepFields fields);

    public StepNode Build(StepFields fields, IReadOnlyList<UnknownFieldNode> unknownFields, SourceSpan span);
}
