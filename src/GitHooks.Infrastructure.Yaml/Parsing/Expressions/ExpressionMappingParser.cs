using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed class ExpressionMappingParser(
    ExpressionParser expressionParser,
    UnknownNodeParser unknownNodeParser)
{
    private readonly ExpressionParser _expressionParser
        = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));

    private readonly UnknownNodeParser _unknownNodeParser
        = unknownNodeParser ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    public MappingFieldNode<StringKeyFieldNode<ExpressionNode>> Parse(
        Scalar key,
        ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        List<StringKeyFieldNode<ExpressionNode>> fields = [];

        var mappingFields = new MappingFields(_unknownNodeParser);

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                // TODO: preserve unknown fields inside mapping
                mappingFields.AddUnknownField(context);

                continue;
            }

            var itemKey = context.Cursor.Read<Scalar>();

            var field = mappingFields.ReadFirst(
                itemKey,
                context,
                currentValue: null,
                readValue: context => ReadField(
                    itemKey,
                    context));

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

    private StringKeyFieldNode<ExpressionNode> ReadField(
        Scalar key,
        ParsingContext context)
    {
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
}
