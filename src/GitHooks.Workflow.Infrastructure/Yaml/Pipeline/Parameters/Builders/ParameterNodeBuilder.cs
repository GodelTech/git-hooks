using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Builders;

internal sealed class ParameterNodeBuilder
    : IParameterNodeBuilder
{
    public bool CanBuild(ParameterFields fields)
    {
        return fields.Name is not null;
    }

    public ParameterNode Build(ParameterFields fields, IReadOnlyList<UnknownFieldNode> unknownFields, SourceSpan span)
    {
        return fields.Name is null
            ? throw new Application.Parsing.Exceptions.PipelineParsingException("Parameter definition must contain 'name'", span)
            : new ParameterNode(
                fields.Name,
                fields.DisplayName,
                fields.Type,
                fields.DefaultValue,
                fields.Values,
                unknownFields,
                span
            );
    }
}
