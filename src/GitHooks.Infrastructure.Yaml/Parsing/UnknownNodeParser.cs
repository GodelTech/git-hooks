using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class UnknownNodeParser
{
    public UnknownFieldNode ParseField(
        YamlParserCursor cursor,
        Scalar key)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(key);

        var value = ParseNode(cursor);

        return new UnknownFieldNode
        {
            Key = key.Value ?? string.Empty,
            Value = value,
            Span = SourceSpan.Combine(
                cursor.CreateSpan(key, key),
                value.Span)
        };
    }

    public UnknownNode ParseNode(
        YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        if (cursor.Is<Scalar>())
        {
            return ParseScalar(cursor);
        }

        if (cursor.Is<SequenceStart>())
        {
            return ParseSequence(cursor);
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (cursor.Is<MappingStart>())
        {
            return ParseMapping(cursor);
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        throw cursor.CreateInvalidOperationException(
            "Unsupported unknown node");
    }

#pragma warning disable CA1822 // Mark members as static
    private UnknownScalarNode ParseScalar(
        YamlParserCursor cursor)
#pragma warning restore CA1822 // Mark members as static
    {
        var scalar = cursor.Read<Scalar>();

        return new UnknownScalarNode
        {
            Value = scalar.Value ?? string.Empty,
            Span = cursor.CreateSpan(
                scalar,
                scalar)
        };
    }

    private UnknownSequenceNode ParseSequence(
        YamlParserCursor cursor)
    {
        var start = cursor.Read<SequenceStart>();

        var items = new List<UnknownNode>();

        while (!cursor.Is<SequenceEnd>())
        {
            items.Add(ParseNode(cursor));
        }

        var end = cursor.Read<SequenceEnd>();

        return new UnknownSequenceNode
        {
            Items = items,
            Span = cursor.CreateSpan(
                start,
                end)
        };
    }

    private UnknownMappingNode ParseMapping(
        YamlParserCursor cursor)
    {
        var start = cursor.Read<MappingStart>();

        var fields = new List<UnknownFieldNode>();

        while (!cursor.Is<MappingEnd>())
        {
            if (!cursor.Is<Scalar>())
            {
                _ = ParseNode(cursor);

                continue;
            }

            var key = cursor.Read<Scalar>();

            fields.Add(ParseField(cursor, key));
        }

        var end = cursor.Read<MappingEnd>();

        return new UnknownMappingNode
        {
            Fields = fields,
            Span = cursor.CreateSpan(
                start,
                end)
        };
    }
}
