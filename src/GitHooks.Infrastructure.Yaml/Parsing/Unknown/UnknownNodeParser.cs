using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Unknown;

internal sealed class UnknownNodeParser
{
    public UnknownFieldNode ParseField(YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var key = ParseNode(cursor);
        var value = ParseNode(cursor);

        return new UnknownComplexFieldNode
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Combine(
                key.Span,
                value.Span)
        };
    }

    public UnknownSimpleFieldNode ParseField(Scalar key, YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(cursor);

        var value = ParseNode(cursor);

        return new UnknownSimpleFieldNode
        {
            Key = key.Value ?? string.Empty,
            Value = value,
            Span = SourceSpan.Combine(
                cursor.CreateSpan(key, key),
                value.Span)
        };
    }

    private UnknownNode ParseNode(YamlParserCursor cursor)
    {
        if (cursor.Is<Scalar>())
        {
            return ParseScalar(cursor);
        }

        if (cursor.Is<SequenceStart>())
        {
            return ParseSequence(cursor);
        }

        if (cursor.Is<MappingStart>())
        {
            return ParseMapping(cursor);
        }

        throw cursor.CreateParsingException(
            "Unsupported unknown node");
    }

#pragma warning disable CA1822 // Mark members as static
    private UnknownScalarNode ParseScalar(YamlParserCursor cursor)
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

    private UnknownSequenceNode ParseSequence(YamlParserCursor cursor)
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

    private UnknownMappingNode ParseMapping(YamlParserCursor cursor)
    {
        var start = cursor.Read<MappingStart>();

        var fields = new List<UnknownFieldNode>();

        while (!cursor.Is<MappingEnd>())
        {
            if (cursor.Is<Scalar>())
            {
                var key = cursor.Read<Scalar>();

                fields.Add(
                    ParseField(
                        key,
                        cursor));
            }
            else
            {
                fields.Add(
                    ParseField(
                        cursor));
            }
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
