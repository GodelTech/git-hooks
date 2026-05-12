using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal sealed class YamlReader(Parser parser, SourceRef source)
{
    private readonly Parser _parser = parser;
    private readonly SourceRef _source = source;

    public static YamlReader Create(string yaml, string sourceName)
    {
        return new(
            new Parser(new StringReader(yaml)),
            new SourceRef(sourceName)
        );
    }

    public T Read<T>()
        where T : ParsingEvent
    {
        return !_parser.TryConsume<T>(out var evt)
            ? throw Error($"Expected {typeof(T).Name}")
            : evt;
    }

    public void Require<T>()
        where T : ParsingEvent
    {
        if (!_parser.TryConsume<T>(out _))
        {
            throw Error($"Expected {typeof(T).Name}");
        }
    }

    /// <summary>Peek: is the next event of type T?.</summary>
    /// <returns></returns>
    public bool Is<T>()
        where T : ParsingEvent
    {
        return _parser.Accept<T>(out _);
    }

    /// <summary>Peek and return the next event of type T without consuming.</summary>
    /// <returns></returns>
    public T Peek<T>()
        where T : ParsingEvent
    {
        return _parser.Accept<T>(out var evt)
            ? evt
            : throw Error($"Expected {typeof(T).Name}");
    }

    public UnknownNode ReadUnknownNode()
    {
        if (TryConsumeSafe<Scalar>(out var scalar))
        {
            return new UnknownScalarNode(
                scalar.Value,
                SpanOf(scalar, scalar)
            );
        }

        if (TryConsumeSafe<AnchorAlias>(out var alias))
        {
            return new UnknownReferenceNode(
                alias.Value.ToString(),
                SpanOf(alias, alias)
            );
        }

        if (TryConsumeSafe<MappingStart>(out var mappingStart))
        {
            var entries = new List<UnknownMappingEntryNode>();

            while (!_parser.Accept<MappingEnd>(out _))
            {
                var key = ReadUnknownNode();
                var value = ReadUnknownNode();

                entries.Add(
                    new UnknownMappingEntryNode(
                        key,
                        value,
                        MergeSpans(key.Span, value.Span)
                    )
                );
            }

            var mappingEnd = Read<MappingEnd>();

            return new UnknownMappingNode(entries, SpanOf(mappingStart, mappingEnd));
        }

        if (TryConsumeSafe<SequenceStart>(out var sequenceStart))
        {
            var items = new List<UnknownNode>();

            while (!_parser.Accept<SequenceEnd>(out _))
            {
                items.Add(ReadUnknownNode());
            }

            var sequenceEnd = Read<SequenceEnd>();

            return new UnknownSequenceNode(items, SpanOf(sequenceStart, sequenceEnd));
        }

        if (TryConsumeSafe<ParsingEvent>(out var evt))
        {
            throw new YamlParseException(
                $"Unsupported token while reading unknown node: {evt.GetType().Name}",
                SpanOf(evt, evt)
            );
        }

        throw Error("Unexpected token while reading unknown node");
    }

    public UnknownFieldNode ReadUnknownField(Scalar key)
    {
        var keyNode = new UnknownScalarNode(key.Value, SpanOf(key, key));

        return ReadUnknownField(keyNode);
    }

    public UnknownFieldNode ReadUnknownField(UnknownNode key)
    {
        var value = ReadUnknownNode();

        return new UnknownFieldNode(
            key,
            value,
            MergeSpans(key.Span, value.Span)
        );
    }

    private bool TryConsumeSafe<T>(out T evt)
        where T : ParsingEvent
    {
        try
        {
            if (_parser.TryConsume<T>(out var consumed))
            {
                evt = consumed;
                return true;
            }

            evt = default!;
            return false;
        }
        catch (EndOfStreamException)
        {
            evt = default!;
            return false;
        }
    }

    public SourceSpan CurrentSpan()
    {
        var mark = _parser.Current?.Start;

        return mark is null
            ? SourceSpan.Unknown(_source) :
            SpanOf(mark.Value, mark.Value);
    }

    public SourceSpan SpanOf(ParsingEvent start, ParsingEvent end)
    {
        return SpanOf(start.Start, end.End);
    }

    private SourceSpan SpanOf(Mark start, Mark end)
    {
        return new SourceSpan(
            _source,
            new SourceLocation(start.Line, start.Column),
            new SourceLocation(end.Line, end.Column)
        );
    }

    private static SourceSpan MergeSpans(SourceSpan start, SourceSpan end)
    {
        return new SourceSpan(start.Source, start.Start, end.End);
    }

    private YamlParseException Error(string message)
    {
        return new YamlParseException(
            $"{message}, got {_parser.Current?.GetType().Name ?? "EOF"}",
            CurrentSpan()
        );
    }
}
