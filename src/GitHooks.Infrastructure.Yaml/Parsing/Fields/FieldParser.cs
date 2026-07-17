using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Fields;

internal sealed class FieldParser(
    ExpressionParser expressionParser,
    FieldValueParser fieldValueParser)
{
    private readonly ExpressionParser _expressionParser
        = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));

    private readonly FieldValueParser _fieldValueParser
        = fieldValueParser ?? throw new ArgumentNullException(nameof(fieldValueParser));

    public StringKeyFieldNode<ExpressionNode> ParseStringKeyField(
        Scalar key,
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        var value = _expressionParser.Parse(context);

        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = key.Value,
            Value = value,
            Span = context.Cursor.CreateSpan(
                key.Start,
                value.Span)
        };
    }

    public SequenceFieldNode<ExpressionNode> ParseSequenceField(
        Scalar key,
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        List<ExpressionNode> items = [];

        var start = context.Cursor.Read<SequenceStart>();

        while (!context.Cursor.Is<SequenceEnd>())
        {
            items.Add(
                _expressionParser.Parse(context));
        }

        var end = context.Cursor.Read<SequenceEnd>();

        return new SequenceFieldNode<ExpressionNode>
        {
            Key = key.Value,
            Items = items,
            Span = context.Cursor.CreateSpan(
                key.Start,
                context.Cursor.CreateSpan(start, end))
        };
    }

    public MappingFieldNode<StringKeyFieldNode<ExpressionNode>> ParseMappingField(
        Scalar key,
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        List<StringKeyFieldNode<ExpressionNode>> fields = [];

        var fieldTracker = new FieldTracker(_fieldValueParser);

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                var unsupportedField = fieldTracker.AddUnknownField(context);

                context.Diagnostics.Report(
                    Diagnostic.Create(
                        DiagnosticDescriptors.MappingKeyMustBeScalar,
                        unsupportedField.Span,
                        key.Value));

                continue;
            }

            var itemKey = context.Cursor.Read<Scalar>();

            var field = fieldTracker.ReadFirst(
                itemKey,
                context,
                null,
                context => ParseStringKeyField(itemKey, context));

            if (field is not null)
            {
                fields.Add(field);
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

        return new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
        {
            Key = key.Value,
            Fields = fields,
            Span = context.Cursor.CreateSpan(
                key.Start,
                context.Cursor.CreateSpan(start, end))
        };
    }
}
