using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

internal static class PipelineParametersParser
{
    public static IReadOnlyList<ParameterNode> Parse(YamlReader reader)
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

        while (!reader.Is<MappingEnd>())
        {
            if (!reader.Is<Scalar>())
            {
                // Non-scalar key: skip the key and its value entirely.
                _ = reader.ReadUnknownNode();
                _ = reader.ReadUnknownNode();
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
                    // Skip unrecognised field values for forward-compatibility.
                    _ = reader.ReadUnknownNode();
                    break;
            }
        }

        var end = reader.Read<MappingEnd>();
        var span = reader.SpanOf(start, end);

        return name is null
            ? throw new YamlParseException("Parameter definition must contain 'name'", span)
            : new ParameterNode(name, displayName, type, defaultValue, values, span);
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
