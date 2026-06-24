using GitHooks.Diagnostics;
using GitHooks.Domain.Common;

namespace GitHooks.Validation;

internal sealed class ValidationContext(
    DiagnosticBag diagnostics)
{
    private readonly DiagnosticBag _diagnostics
        = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

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
