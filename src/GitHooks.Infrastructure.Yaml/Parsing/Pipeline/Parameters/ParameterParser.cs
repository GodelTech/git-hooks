using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;

internal sealed class ParameterParser(
    FieldParser fieldParser,
    FieldValueParser fieldValueParser)
{
    private readonly FieldParser _fieldParser
        = fieldParser ?? throw new ArgumentNullException(nameof(fieldParser));

    private readonly FieldValueParser _fieldValueParser
        = fieldValueParser ?? throw new ArgumentNullException(nameof(fieldValueParser));

    public ParameterNode Parse(ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        var fieldTracker = new FieldTracker(_fieldValueParser);

        StringKeyFieldNode<ExpressionNode>? name = null;
        StringKeyFieldNode<ExpressionNode>? displayName = null;
        var type = ParameterType.String;
        StringKeyFieldNode<ExpressionNode>? defaultValue = null;
        SequenceFieldNode<ExpressionNode>? values = null;

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                fieldTracker.AddUnknownField(context);

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "name":
                    name = fieldTracker.ReadFirst(
                        key,
                        context,
                        name,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "displayname":
                    displayName = fieldTracker.ReadFirst(
                        key,
                        context,
                        displayName,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "type":
                    type = fieldTracker.ReadFirst(
                        key,
                        context,
                        type,
                        static context => ParseType(
                            context.Cursor.Read<Scalar>().Value,
                            context));
                    break;

                case "default":
                    defaultValue = fieldTracker.ReadFirst(
                        key,
                        context,
                        defaultValue,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "values":
                    values = fieldTracker.ReadFirst(
                        key,
                        context,
                        values,
                        context => _fieldParser.ParseSequenceField(key, context));
                    break;

                default:
                    fieldTracker.AddUnknownField(key, context);
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

            name = MissingFields.StringKeyField(
                "name",
                span);
        }

        return new ParameterNode
        {
            Name = name,
            DisplayName = displayName,
            Type = type,
            DefaultValue = defaultValue,
            Values = values,
            UnknownFields = [.. fieldTracker.GetUnknownFields()],
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
}
