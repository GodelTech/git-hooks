using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;

internal sealed class ParameterParser(
    UnknownNodeParser unknownNodeParser)
{
    private static readonly IReadOnlyList<string> s_emptyValues
        = [];

    private readonly UnknownNodeParser _unknownNodeParser
        = unknownNodeParser ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    public ParameterNode Parse(YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var start = cursor.Read<MappingStart>();

        var fields = new MappingFields(_unknownNodeParser);

        string? name = null;
        string? displayName = null;
        var type = ParameterType.String;
        string? defaultValue = null;
        List<string>? values = null;

        while (!cursor.Is<MappingEnd>())
        {
            if (!cursor.Is<Scalar>())
            {
                throw cursor.CreateParsingException(
                    "Expected scalar mapping key");
            }

            var key = cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "name":
                    fields.MarkSeen(key, cursor);
                    name = cursor.Read<Scalar>().Value;
                    break;

                case "displayname":
                    fields.MarkSeen(key, cursor);
                    displayName = cursor.Read<Scalar>().Value;
                    break;

                case "type":
                    fields.MarkSeen(key, cursor);
                    type = ParseType(cursor.Read<Scalar>().Value, cursor);
                    break;

                case "default":
                    fields.MarkSeen(key, cursor);
                    defaultValue = cursor.Read<Scalar>().Value;
                    break;

                case "values":
                    fields.MarkSeen(key, cursor);
                    values = ParseValues(cursor);
                    break;

                default:
                    fields.AddUnknownField(key, cursor);
                    break;
            }
        }

        var end = cursor.Read<MappingEnd>();

        var span = cursor.CreateSpan(start, end);

        if (name is null)
        {
            throw cursor.CreateParsingException(
                "Parameter requires 'name'");
        }

        return new ParameterNode
        {
            Name = name,
            DisplayName = displayName,
            Type = type,
            DefaultValue = defaultValue,
            Values = values ?? s_emptyValues,
            UnknownFields = fields.UnknownFields,
            Span = span
        };
    }

    private static ParameterType ParseType(
        string value,
        YamlParserCursor cursor)
    {
        return value.ToLowerInvariant() switch
        {
            "string" => ParameterType.String,
            "boolean" => ParameterType.Boolean,
            "number" => ParameterType.Integer,
            "object" => ParameterType.Object,

            _ => throw cursor.CreateParsingException(
                $"Unsupported parameter type '{value}'")
        };
    }

    private static List<string> ParseValues(YamlParserCursor cursor)
    {
        var values = new List<string>();

        _ = cursor.Read<SequenceStart>();

        while (!cursor.Is<SequenceEnd>())
        {
            var value = cursor.Read<Scalar>().Value;

            values.Add(value);
        }

        _ = cursor.Read<SequenceEnd>();

        return values;
    }
}
