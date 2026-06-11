using GitHooks.Diagnostics;
using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class ParsingContext
{
    private readonly DiagnosticBag _diagnostics;

    public ParsingContext(
        string yaml,
        SourceDocument sourceDocument)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(sourceDocument);

        _diagnostics = new DiagnosticBag();

        Cursor = YamlParserCursor.Create(
            yaml,
            sourceDocument);
    }

    public IReadOnlyList<Diagnostic> Diagnostics
        => _diagnostics.Diagnostics;

    public YamlParserCursor Cursor { get; }

    public void Report(
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        _diagnostics.Report(
            descriptor,
            span,
            arguments);
    }
}
