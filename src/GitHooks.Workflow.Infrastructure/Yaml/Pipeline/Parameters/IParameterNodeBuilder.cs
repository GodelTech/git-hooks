using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

internal interface IParameterNodeBuilder
{
    public bool CanBuild(ParameterFields fields);

    public ParameterNode Build(ParameterFields fields, IReadOnlyList<UnknownFieldNode> unknownFields, SourceSpan span);
}
