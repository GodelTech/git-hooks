using GitHooks.Diagnostics;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Parsing;

public sealed class ParsingContext(
    string text,
    SourceDocument document,
    DiagnosticBag diagnostics)
{
    public string Text { get; }
        = text ?? throw new ArgumentNullException(nameof(text));

    public SourceDocument Document { get; }
        = document ?? throw new ArgumentNullException(nameof(document));

    public DiagnosticBag Diagnostics { get; }
        = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
}
