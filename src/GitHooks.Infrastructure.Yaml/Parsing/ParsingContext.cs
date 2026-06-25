using GitHooks.Diagnostics;
using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class ParsingContext
{
    private readonly DiagnosticBag _diagnostics;

    public ParsingContext(
        string yaml,
        SourceDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(document);

        _diagnostics = new DiagnosticBag();

        Cursor = YamlParserCursor.Create(
            yaml,
            document);
    }

    public IReadOnlyList<Diagnostic> Diagnostics
        => _diagnostics.Diagnostics;

    public YamlParserCursor Cursor { get; }

    public void Report(
        Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        _diagnostics.Report(diagnostic);
    }
}
