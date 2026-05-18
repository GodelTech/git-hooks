using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding.Exceptions;

namespace GitHooks.Workflow.Application.Binding;

internal sealed class UnknownNodeParameterBinder(StringBinder stringBinder)
{
    private readonly StringBinder _stringBinder = stringBinder;

    public UnknownFieldNode[] BindUnknownFields(
        IReadOnlyList<UnknownFieldNode> unknownFields,
        ParameterBindingContext context)
    {
        return [.. unknownFields
            .Select(
                field => field with
                {
                    Key = BindUnknownNode(field.Key, context),
                    Value = BindUnknownNode(field.Value, context)
                }
            )
        ];
    }

    private UnknownNode BindUnknownNode(UnknownNode node, ParameterBindingContext context)
    {
        return node switch
        {
            UnknownScalarNode scalarNode => scalarNode with
            {
                Value = _stringBinder.Bind(scalarNode.Value, scalarNode.Span, context)
            },
            UnknownMappingNode mappingNode => mappingNode with
            {
                Entries = [.. mappingNode.Entries
                    .Select(
                        entry => entry with
                        {
                            Key = BindUnknownNode(entry.Key, context),
                            Value = BindUnknownNode(entry.Value, context)
                        }
                    )
                ]
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
