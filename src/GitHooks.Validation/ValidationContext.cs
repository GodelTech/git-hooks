using GitHooks.Diagnostics;
using GitHooks.Validation.Symbols;

namespace GitHooks.Validation;

internal sealed class ValidationContext(
    DiagnosticBag diagnostics)
{
    private readonly DiagnosticBag _diagnostics
        = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

    public SymbolTable Symbols { get; }
        = new();

    public void Report(
        Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        _diagnostics.Report(diagnostic);
    }
}
