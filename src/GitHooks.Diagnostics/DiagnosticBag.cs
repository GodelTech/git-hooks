namespace GitHooks.Diagnostics;

public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics
        = [];

    // todo: rename to Items or implement IReadOnlyList<Diagnostic>
    public IReadOnlyList<Diagnostic> Diagnostics
        => _diagnostics;

    public int Count =>
        _diagnostics.Count;

    public bool HasErrors
        => _diagnostics.Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public Diagnostic this[int index] =>
        _diagnostics[index];

    public void Report(
        Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        _diagnostics.Add(diagnostic);
    }
}
