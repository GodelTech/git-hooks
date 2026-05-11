using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

internal sealed class PipelineParametersParser
{
#pragma warning disable CA1822 // Parser is intentionally instance-based for DI consistency with other pipeline parsers.
    public IReadOnlyList<ParameterNode> Parse(YamlReader reader)
#pragma warning restore CA1822
    {
        _ = reader.Read<SequenceStart>();

        var parameters = new List<ParameterNode>();

        while (!reader.Is<SequenceEnd>())
        {
            var parameter = ParseOne(reader);
            parameters.Add(parameter);
        }

        _ = reader.Read<SequenceEnd>();

        return parameters;
    }

    private static ParameterNode ParseOne(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        string? name = null;
        string? displayName = null;
        var type = ParameterType.Text;
        string? defaultValue = null;
        var values = new List<string>();
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

            switch (key.Value)
            {
                case "name":
                    name = reader.Read<Scalar>().Value;
                    break;

                case "displayName":
                    displayName = reader.Read<Scalar>().Value;
                    break;

                case "type":
                    type = ParseParameterType(reader.Read<Scalar>().Value);
                    break;

                case "default":
                    // Default may be any scalar: string, boolean literal, or number.
                    defaultValue = reader.Read<Scalar>().Value;
                    break;

                case "values":
                    values = ParseValuesList(reader);
                    break;

                default:
                    unknownFields.Add(reader.ReadUnknownField(key));
                    break;
            }
        }

        var end = reader.Read<MappingEnd>();
        var span = reader.SpanOf(start, end);

        return name is null
            ? throw new YamlParseException("Parameter definition must contain 'name'", span)
            : new ParameterNode(name, displayName, type, defaultValue, values, unknownFields, span);
    }

    private static List<string> ParseValuesList(YamlReader reader)
    {
        _ = reader.Read<SequenceStart>();

        var values = new List<string>();

        while (!reader.Is<SequenceEnd>())
        {
            values.Add(reader.Read<Scalar>().Value);
        }

        _ = reader.Read<SequenceEnd>();

        return values;
    }

    private static ParameterType ParseParameterType(string value)
    {
        return value switch
        {
            "boolean" => ParameterType.Boolean,
            "number" => ParameterType.Number,
            "object" => ParameterType.Mapping,
            _ => ParameterType.Text
        };
    }
}

