using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Parameters;

namespace GitHooks.Workflow.Application.Binding.UnknownNodes;

internal sealed class UnknownNodeParameterBinder
{
    public static UnknownFieldNode[] BindUnknownFields(
        IReadOnlyList<UnknownFieldNode> unknownFields,
        ParameterBindingContext context)
    {
        return [.. unknownFields.Select(field => field with
        {
            Key = BindUnknownNode(field.Key, context),
            Value = BindUnknownNode(field.Value, context)
        })];
    }

    public static UnknownNode BindUnknownNode(UnknownNode node, ParameterBindingContext context)
    {
        return node switch
        {
            UnknownScalarNode scalarNode => scalarNode with
            {
                Value = ParameterScalarBinder.Bind(scalarNode.Value, scalarNode.Span, context)
            },
            UnknownMappingNode mappingNode => mappingNode with
            {
                Entries = [.. mappingNode.Entries.Select(entry => entry with
                {
                    Key = BindUnknownNode(entry.Key, context),
                    Value = BindUnknownNode(entry.Value, context)
                })]
            },
            UnknownSequenceNode sequenceNode => sequenceNode with
            {
                Items = [.. sequenceNode.Items.Select(item => BindUnknownNode(item, context))]
            },
            UnknownReferenceNode referenceNode => referenceNode,
            _ => throw new PipelineParameterBindingException(
                $"Unsupported unknown node type '{node.GetType().Name}'.",
                node.Span
            )
        };
    }
}
