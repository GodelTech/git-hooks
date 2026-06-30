using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
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
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        _diagnostics.Report(
            Diagnostic.Create(
                descriptor,
                span,
                arguments));
    }
}
