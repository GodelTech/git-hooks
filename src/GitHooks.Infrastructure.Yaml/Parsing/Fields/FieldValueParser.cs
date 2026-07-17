using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Common;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Fields;

internal sealed class FieldValueParser
{
    public StringKeyFieldNode<ValueNode> ParseStringKeyField(
        Scalar key,
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        var value = ParseValue(context);

        return new StringKeyFieldNode<ValueNode>
        {
            Key = key.Value,
            Value = value,
            Span = SourceSpan.Combine(
                context.Cursor.CreateSpan(key, key),
                value.Span)
        };
    }

    public ComplexKeyFieldNode<ValueNode> ParseComplexKeyField(
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var key = ParseValue(context);
        var value = ParseValue(context);

        return new ComplexKeyFieldNode<ValueNode>
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Combine(
                key.Span,
                value.Span)
        };
    }

    private ValueNode ParseValue(
        YamlParserContext context)
    {
        if (context.Cursor.Is<Scalar>())
        {
            return ParseScalar(context);
        }

        if (context.Cursor.Is<SequenceStart>())
        {
            return ParseSequence(context);
        }

        if (context.Cursor.Is<MappingStart>())
        {
            return ParseMapping(context);
        }

        throw context.Cursor.CreateException(
            "Unexpected YAML node. Expected scalar, sequence, or mapping.");
    }

#pragma warning disable CA1822 // Mark members as static
    private ScalarNode ParseScalar(
        YamlParserContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        var scalar = context.Cursor.Read<Scalar>();

        return new ScalarNode
        {
            Value = scalar.Value,
            Span = context.Cursor.CreateSpan(
                scalar,
                scalar)
        };
    }

    private SequenceNode ParseSequence(
        YamlParserContext context)
    {
        var start = context.Cursor.Read<SequenceStart>();

        List<ValueNode> items = [];

        while (!context.Cursor.Is<SequenceEnd>())
        {
            items.Add(
                ParseValue(context));
        }

        var end = context.Cursor.Read<SequenceEnd>();

        return new SequenceNode
        {
            Items = items,
            Span = context.Cursor.CreateSpan(
                start,
                end)
        };
    }

    private MappingNode ParseMapping(
        YamlParserContext context)
    {
        var start = context.Cursor.Read<MappingStart>();

        List<FieldNode> fields = [];

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                fields.Add(
                    ParseComplexKeyField(context));

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            fields.Add(
                ParseStringKeyField(key, context));
        }

        var end = context.Cursor.Read<MappingEnd>();

        return new MappingNode
        {
            Fields = fields,
            Span = context.Cursor.CreateSpan(
                start,
                end)
        };
    }
}
