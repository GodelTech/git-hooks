using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlParserCursor(
    Parser parser,
    string sourceName)
{
    private readonly Parser _parser
        = parser
        ?? throw new ArgumentNullException(nameof(parser));

    private readonly string _sourceName
        = sourceName
        ?? throw new ArgumentNullException(nameof(sourceName));

    public static YamlParserCursor Create(
        string yaml,
        string sourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        return new(
            new Parser(new StringReader(yaml)),
            sourceName);
    }

    public T Read<T>()
        where T : ParsingEvent
    {
        return !_parser.TryConsume<T>(out var parsingEvent)
            ? throw CreateParsingException($"Expected {typeof(T).Name}")
            : parsingEvent;
    }

    public bool Is<T>()
        where T : ParsingEvent
    {
        return _parser.Accept<T>(out _);
    }

    public SourceSpan CreateSpan(
        ParsingEvent start,
        ParsingEvent end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        return CreateSpan(
            start.Start,
            end.End);
    }

    public SourceSpan CreateSpan(
        YamlException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return CreateSpan(
            exception.Start,
            exception.End);
    }

#pragma warning disable CA1822 // Mark members as static
    public SourceSpan CreateSpan(
        Mark start,
        Mark end)
#pragma warning restore CA1822 // Mark members as static
    {
        return new SourceSpan(
            new SourcePosition(
                start.Line,
                start.Column),
            new SourcePosition(
                end.Line,
                end.Column));
    }

    public SourceSpan CurrentSpan()
    {
        var mark = _parser.Current?.Start;

        return mark is null
            ? SourceSpan.Unknown
            : CreateSpan(mark.Value, mark.Value);
    }

    internal YamlPipelineParsingException CreateParsingException(string message)
    {
        var span = CurrentSpan();

        return new YamlPipelineParsingException(
            $"{message}, " +
            $"got {_parser.Current?.GetType().Name ?? "EOF"} " +
            $"in {_sourceName} " +
            $"at {span.Start.Line}:{span.Start.Column}",
            span
        );
    }
}
