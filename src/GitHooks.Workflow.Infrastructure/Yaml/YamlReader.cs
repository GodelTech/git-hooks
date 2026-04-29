using GitHooks.Workflow.Domain.Model;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal sealed class YamlReader(Parser parser, SourceRef source)
{
    private readonly Parser _parser = parser;
    private readonly SourceRef _source = source;

    public SourceSpan CurrentSpan()
    {
        var mark = _parser.Current?.Start;

        return mark is null
            ? SourceSpan.Unknown(_source) :
            (mark.Value, mark.Value).ToSourceSpan(_source);
    }

    public SourceSpan SpanOf(ParsingEvent start, ParsingEvent end)
    {
        return (start.Start, end.End).ToSourceSpan(_source);
    }

    /// <summary>Consume and return the next event of type T; throws if not present.</summary>
    public T Read<T>() where T : ParsingEvent
    {
        return !_parser.TryConsume<T>(out var evt)
            ? throw Error($"Expected {typeof(T).Name}")
            : evt;
    }

    /// <summary>Consume the next event of type T; throws if not present.</summary>
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
        if (_parser.TryConsume<Scalar>(out _))
        {
            return;
        }

        // mapping
        if (_parser.TryConsume<MappingStart>(out _))
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
        if (_parser.TryConsume<SequenceStart>(out _))
        {
            while (!_parser.Accept<SequenceEnd>(out _))
            {
                SkipNode();
            }

            Require<SequenceEnd>();
            return;
        }

        // fallback (unknown token)
        _ = _parser.TryConsume<ParsingEvent>(out _);
    }

    private YamlParseException Error(string message)
    {
        return new YamlParseException(
            $"{message}, got {_parser.Current?.GetType().Name ?? "EOF"}",
            CurrentSpan()
        );
    }
}
