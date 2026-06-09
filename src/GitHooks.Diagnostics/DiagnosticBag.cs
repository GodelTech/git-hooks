using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics;

public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics
        = [];

    public IReadOnlyList<Diagnostic> Diagnostics
        => _diagnostics;

    public void Report(
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(arguments);

        _diagnostics.Add(
            Diagnostic.Create(
                descriptor,
                span,
                arguments));
    }
}
