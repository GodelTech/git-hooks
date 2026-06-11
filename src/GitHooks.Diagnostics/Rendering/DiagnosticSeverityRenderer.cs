namespace GitHooks.Diagnostics.Rendering;

public static class DiagnosticSeverityRenderer
{
    public static string Render(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Info => "info",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Error => "error",

            _ => throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                null)
        };
    }
}
