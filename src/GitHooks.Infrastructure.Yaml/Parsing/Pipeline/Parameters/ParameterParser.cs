using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;

internal sealed class ParameterParser(
    ExpressionParser expressionParser,
    UnknownNodeParser unknownNodeParser)
{
    private static readonly IReadOnlyList<ExpressionNode> s_emptyValues
        = [];

    private readonly ExpressionParser _expressionParser
        = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));

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
        ExpressionNode? defaultValue = null;
        List<ExpressionNode>? values = null;

        while (!cursor.Is<MappingEnd>())
        {
            if (!cursor.Is<Scalar>())
            {
                fields.AddUnknownField(cursor);

                continue;
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
                    defaultValue = _expressionParser.Parse(cursor);
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

        if (name is null)
        {
            throw cursor.CreateException(
                "Parameter requires 'name'");
        }

        var span = cursor.CreateSpan(start, end);

        return new ParameterNode
        {
            Name = name,
            DisplayName = displayName,
            Type = type,
            DefaultValue = defaultValue,
            Values = values ?? s_emptyValues,
            UnknownFields = fields.GetUnknownFields(),
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
            "number" => ParameterType.Number,
            "object" => ParameterType.Object,

            _ => throw cursor.CreateException(
                $"Unsupported parameter type '{value}'")
        };
    }

    private List<ExpressionNode> ParseValues(YamlParserCursor cursor)
    {
        var values = new List<ExpressionNode>();

        _ = cursor.Read<SequenceStart>();

        while (!cursor.Is<SequenceEnd>())
        {
            var value = _expressionParser.Parse(cursor);

            values.Add(value);
        }

        _ = cursor.Read<SequenceEnd>();

        return values;
    }
}
