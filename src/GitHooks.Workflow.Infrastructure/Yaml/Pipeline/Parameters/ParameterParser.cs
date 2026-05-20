using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

internal sealed class ParameterParser(IEnumerable<IParameterFieldHandler> handlers, IParameterNodeBuilder builder)
{
    private readonly Dictionary<string, IParameterFieldHandler> _handlers = handlers.ToDictionary(h => h.Key, StringComparer.Ordinal);
    private readonly IParameterNodeBuilder _builder = builder;

    public ParameterNode Parse(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        var fields = new ParameterFields();
        var unknownFields = new List<UnknownFieldNode>();

        while (!reader.Is<MappingEnd>())
        {
            if (!reader.Is<Scalar>())
            {
                var keyNode = reader.ReadUnknownNode();
                unknownFields.Add(reader.ReadUnknownField(keyNode));
                continue;
            }

            var key = reader.Read<Scalar>();

            if (_handlers.TryGetValue(key.Value, out var handler))
            {
                fields = handler.Apply(reader, fields);
            }
            else
            {
                unknownFields.Add(reader.ReadUnknownField(key));
            }
        }

        var end = reader.Read<MappingEnd>();

        var span = reader.SpanOf(start, end);

        return BuildParameter(fields, unknownFields, span);
    }

    private ParameterNode BuildParameter(ParameterFields fields, IReadOnlyList<UnknownFieldNode> unknownFields, SourceSpan span)
    {
        return !_builder.CanBuild(fields)
            ? throw new Application.Parsing.Exceptions.PipelineParsingException(
                "Parameter definition must contain 'name'",
                span
            )
            : _builder.Build(fields, unknownFields, span);
    }
}
