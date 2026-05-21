using System.Text;

using GitHooks.Diagnostics.Rendering;

namespace GitHooks.Diagnostics.Printing;

public sealed class DiagnosticPrinter(
    DiagnosticPrinterOptions? options = null)
{
    private readonly DiagnosticPrinterOptions _options
        = options ?? DiagnosticPrinterOptions.Default;

    public string Print(
        IReadOnlyList<Diagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var builder = new StringBuilder();

        foreach (var diagnostic in diagnostics)
        {
            _ = builder.Append(FormatDiagnostic(diagnostic));
            _ = builder.Append('\n');
        }

        return builder.ToString().TrimEnd();
    }

    private string FormatDiagnostic(
        Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        var builder = new StringBuilder();

        if (_options.IncludeSeverity)
        {
            _ = builder.Append(
                $"{diagnostic.Severity} ");
        }

        if (_options.IncludeCodes)
        {
            _ = builder.Append(
                $"{diagnostic.Code}: ");
        }

        _ = builder.Append(diagnostic.Message);

        if (_options.IncludeSourceSpans)
        {
            _ = builder.Append(
                $" @ {SourceSpanRenderer.Render(diagnostic.Span)}");
        }

        return builder.ToString();
    }
}
