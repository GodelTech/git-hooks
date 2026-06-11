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

    public ParameterNode Parse(ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        var fields = new MappingFields(_unknownNodeParser);

        string? name = null;
        string? displayName = null;
        var type = ParameterType.String;
        ExpressionNode? defaultValue = null;
        List<ExpressionNode>? values = null;

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                fields.AddUnknownField(context);

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "name":
                    fields.MarkSeen(key, context);
                    name = context.Cursor.Read<Scalar>().Value;
                    break;

                case "displayname":
                    fields.MarkSeen(key, context);
                    displayName = context.Cursor.Read<Scalar>().Value;
                    break;

                case "type":
                    fields.MarkSeen(key, context);
                    type = ParseType(context.Cursor.Read<Scalar>().Value, context);
                    break;

                case "default":
                    fields.MarkSeen(key, context);
                    defaultValue = _expressionParser.Parse(context);
                    break;

                case "values":
                    fields.MarkSeen(key, context);
                    values = ParseValues(context);
                    break;

                default:
                    fields.AddUnknownField(key, context);
                    break;
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

        if (name is null)
        {
            throw context.Cursor.CreateException(
                "Parameter requires 'name'");
        }

        var span = context.Cursor.CreateSpan(start, end);

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
        ParsingContext context)
    {
        return value.ToLowerInvariant() switch
        {
            "string" => ParameterType.String,
            "boolean" => ParameterType.Boolean,
            "number" => ParameterType.Number,
            "object" => ParameterType.Object,

            _ => throw context.Cursor.CreateException(
                $"Unsupported parameter type '{value}'")
        };
    }

    private List<ExpressionNode> ParseValues(ParsingContext context)
    {
        var values = new List<ExpressionNode>();

        _ = context.Cursor.Read<SequenceStart>();

        while (!context.Cursor.Is<SequenceEnd>())
        {
            var value = _expressionParser.Parse(context);

            values.Add(value);
        }

        _ = context.Cursor.Read<SequenceEnd>();

        return values;
    }
}
