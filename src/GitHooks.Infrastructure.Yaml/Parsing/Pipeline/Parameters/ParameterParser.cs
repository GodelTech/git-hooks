using GitHooks.Diagnostics;
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
                    name = fields.ReadFirst(
                        key,
                        context,
                        name,
                        static context => context.Cursor.Read<Scalar>().Value);
                    break;

                case "displayname":
                    displayName = fields.ReadFirst(
                        key,
                        context,
                        displayName,
                        static context => context.Cursor.Read<Scalar>().Value);
                    break;

                case "type":
                    type = fields.ReadFirst(
                        key,
                        context,
                        type,
                        static context => ParseType(
                            context.Cursor.Read<Scalar>().Value,
                            context));
                    break;

                case "default":
                    defaultValue = fields.ReadFirst(
                        key,
                        context,
                        defaultValue,
                        _expressionParser.Parse);
                    break;

                case "values":
                    values = fields.ReadFirst(
                        key,
                        context,
                        values,
                        ParseValues);
                    break;

                default:
                    fields.AddUnknownField(key, context);
                    break;
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

        var span = context.Cursor.CreateSpan(start, end);

        if (name is null)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterNameRequired,
                    span));

            name = string.Empty;
        }

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
        switch (value.ToLowerInvariant())
        {
            case "string":
                return ParameterType.String;

            case "boolean":
                return ParameterType.Boolean;

            case "number":
                return ParameterType.Number;

            case "object":
                return ParameterType.Object;

            default:
                context.Report(
                    Diagnostic.Create(
                        DiagnosticDescriptors.UnsupportedParameterType,
                        context.Cursor.CurrentSpan(),
                        value));

                return ParameterType.String;
        }
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
