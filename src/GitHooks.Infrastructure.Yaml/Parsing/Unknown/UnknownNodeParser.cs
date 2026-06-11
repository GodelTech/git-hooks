using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Unknown;

internal sealed class UnknownNodeParser
{
    public UnknownFieldNode ParseField(ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var key = ParseNode(context);
        var value = ParseNode(context);

        return new UnknownComplexFieldNode
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Combine(
                key.Span,
                value.Span)
        };
    }

    public UnknownSimpleFieldNode ParseField(Scalar key, ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);

        var value = ParseNode(context);

        return new UnknownSimpleFieldNode
        {
            Key = key.Value,
            Value = value,
            Span = SourceSpan.Combine(
                context.Cursor.CreateSpan(key, key),
                value.Span)
        };
    }

    private UnknownNode ParseNode(ParsingContext context)
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
    private UnknownScalarNode ParseScalar(ParsingContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        var scalar = context.Cursor.Read<Scalar>();

        return new UnknownScalarNode
        {
            Value = scalar.Value,
            Span = context.Cursor.CreateSpan(
                scalar,
                scalar)
        };
    }

    private UnknownSequenceNode ParseSequence(ParsingContext context)
    {
        var start = context.Cursor.Read<SequenceStart>();

        var items = new List<UnknownNode>();

        while (!context.Cursor.Is<SequenceEnd>())
        {
            items.Add(ParseNode(context));
        }

        var end = context.Cursor.Read<SequenceEnd>();

        return new UnknownSequenceNode
        {
            Items = items,
            Span = context.Cursor.CreateSpan(
                start,
                end)
        };
    }

    private UnknownMappingNode ParseMapping(ParsingContext context)
    {
        var start = context.Cursor.Read<MappingStart>();

        var fields = new List<UnknownFieldNode>();

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

        return new UnknownMappingNode
        {
            Fields = fields,
            Span = context.Cursor.CreateSpan(
                start,
                end)
        };
    }
}
