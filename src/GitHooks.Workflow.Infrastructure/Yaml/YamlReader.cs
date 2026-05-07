using GitHooks.Workflow.Domain.Model;

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

    public T Read<T>() where T : ParsingEvent
    {
        return !_parser.TryConsume<T>(out var evt)
            ? throw Error($"Expected {typeof(T).Name}")
            : evt;
    }

    public void Require<T>() where T : ParsingEvent
    {
        if (!_parser.TryConsume<T>(out _))
        {
            throw Error($"Expected {typeof(T).Name}");
        }
    }

    /// <summary>Peek: is the next event of type T?</summary>
    public bool Is<T>() where T : ParsingEvent
    {
        return _parser.Accept<T>(out _);
    }

    /// <summary>Peek and return the next event of type T without consuming.</summary>
    public T Peek<T>() where T : ParsingEvent
    {
        return _parser.Accept<T>(out var evt)
            ? evt
            : throw Error($"Expected {typeof(T).Name}");
    }

    /// <summary>Skipping (robust, no MoveNext).</summary>
    public void SkipNode()
    {
        // scalar (fast path)
        if (TryConsumeSafe<Scalar>(out _))
        {
            return;
        }

        // mapping
        if (TryConsumeSafe<MappingStart>(out _))
        {
            while (!_parser.Accept<MappingEnd>(out _))
            {
                _ = Read<Scalar>(); // key
                SkipNode();         // value
            }

            Require<MappingEnd>();
            return;
        }

        // sequence
        if (TryConsumeSafe<SequenceStart>(out _))
        {
            while (!_parser.Accept<SequenceEnd>(out _))
            {
                SkipNode();
            }

            Require<SequenceEnd>();
            return;
        }

        // unknown token
        if (!TryConsumeSafe<ParsingEvent>(out _))
        {
            throw Error("Unexpected token while skipping node");
        }
    }

    private bool TryConsumeSafe<T>(out T evt) where T : ParsingEvent
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

    private YamlParseException Error(string message)
    {
        return new YamlParseException(
            $"{message}, got {_parser.Current?.GetType().Name ?? "EOF"}",
            CurrentSpan()
        );
    }
}
