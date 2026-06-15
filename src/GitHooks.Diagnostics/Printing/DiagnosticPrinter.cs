using System.Text;

using GitHooks.Diagnostics.Rendering;

namespace GitHooks.Diagnostics.Printing;

// todo: show RelatedLocations
public sealed class DiagnosticPrinter(
    DiagnosticPrinterOptions? options = null)
{
    private readonly DiagnosticPrinterOptions _options
        = options ?? DiagnosticPrinterOptions.Default;

    public string Print(IReadOnlyList<Diagnostic> diagnostics)
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

    private string FormatDiagnostic(Diagnostic diagnostic)
    {
        var builder = new StringBuilder();

        if (_options.IncludeSourceSpans)
        {
            _ = builder.Append(DiagnosticLocationRenderer.Render(diagnostic.Span));
            _ = builder.Append(": ");
        }

        if (_options.IncludeSeverity)
        {
            _ = builder.Append(DiagnosticSeverityRenderer.Render(diagnostic.Severity));
            _ = builder.Append(' ');
        }

        if (_options.IncludeCodes)
        {
            _ = builder.Append(diagnostic.Code);
            _ = builder.Append(": ");
        }

        _ = builder.Append(diagnostic.Message);

        return builder.ToString();
    }
}
