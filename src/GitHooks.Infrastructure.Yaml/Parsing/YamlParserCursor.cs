using GitHooks.Domain.Common;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlParserCursor(
    Parser parser,
    SourceDocument document)
{
    private readonly Parser _parser
        = parser ?? throw new ArgumentNullException(nameof(parser));

    private readonly SourceDocument _document
        = document ?? throw new ArgumentNullException(nameof(document));

    public static YamlParserCursor Create(
        string yaml,
        SourceDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(document);

        return new(
            new Parser(
                new StringReader(yaml)),
            document);
    }

    public T Read<T>()
        where T : ParsingEvent
    {
        if (_parser.TryConsume<T>(out var parsingEvent))
        {
            return parsingEvent;
        }

        // Guaranteed by YamlDotNet ParserExtensions.TryConsume contract.
        var current = _parser.Current!;

        throw CreateException(
            $"Expected {typeof(T).Name}, got {current.GetType().Name}");
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

    public SourceSpan CreateSpan(
        Mark start,
        Mark end)
    {
        return new SourceSpan(
            _document,
            new SourcePosition(
                start.Line,
                start.Column),
            new SourcePosition(
                end.Line,
                end.Column));
    }

    public SourceSpan CreateSpan(
        Mark start,
        SourceSpan end)
    {
        return new SourceSpan(
            _document,
            new SourcePosition(
                start.Line,
                start.Column),
            end.End);
    }

    public SourceSpan CurrentSpan()
    {
        var mark = _parser.Current?.Start;

        return mark is null
            ? SourceSpan.Unknown
            : CreateSpan(mark.Value, mark.Value);
    }

    internal YamlException CreateException(
        string message)
    {
        return new YamlException(
            _parser.Current?.Start ?? Mark.Empty,
            _parser.Current?.End ?? Mark.Empty,
            message);
    }
}
