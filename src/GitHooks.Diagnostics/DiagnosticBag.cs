namespace GitHooks.Diagnostics;

public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics
        = [];

    public IReadOnlyList<Diagnostic> Diagnostics
        => _diagnostics;

    public void Report(Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        _diagnostics.Add(diagnostic);
    }
}
