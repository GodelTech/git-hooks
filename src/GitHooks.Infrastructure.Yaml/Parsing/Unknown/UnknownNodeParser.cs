using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Common;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Unknown;

// todo: split into FieldParser and ValueParser
internal sealed class UnknownNodeParser
{
    public ComplexKeyFieldNode<ValueNode> ParseField(ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var key = ParseNode(context);
        var value = ParseNode(context);

        return new ComplexKeyFieldNode<ValueNode>
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Combine(
                key.Span,
                value.Span)
        };
    }

    public StringKeyFieldNode<ValueNode> ParseField(Scalar key, ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        var value = ParseNode(context);

        return new StringKeyFieldNode<ValueNode>
        {
            Key = key.Value,
            Value = value,
            Span = SourceSpan.Combine(
                context.Cursor.CreateSpan(key, key),
                value.Span)
        };
    }

    private ValueNode ParseNode(ParsingContext context)
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
            "Unsupported unknown node");
    }

#pragma warning disable CA1822 // Mark members as static
    private ScalarNode ParseScalar(ParsingContext context)
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

    private SequenceNode ParseSequence(ParsingContext context)
    {
        var start = context.Cursor.Read<SequenceStart>();

        var items = new List<ValueNode>();

        while (!context.Cursor.Is<SequenceEnd>())
        {
            items.Add(ParseNode(context));
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

    private MappingNode ParseMapping(ParsingContext context)
    {
        var start = context.Cursor.Read<MappingStart>();

        var fields = new List<FieldNode>();

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (context.Cursor.Is<Scalar>())
            {
                var key = context.Cursor.Read<Scalar>();

                fields.Add(
                    ParseField(
                        key,
                        context));
            }
            else
            {
                fields.Add(
                    ParseField(
                        context));
            }
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
