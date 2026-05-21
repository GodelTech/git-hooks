namespace GitHooks.Diagnostics.Printing;

public sealed record DiagnosticPrinterOptions
{
    public static DiagnosticPrinterOptions Default { get; }
        = new();

    public bool IncludeSeverity { get; init; } = true;

    public bool IncludeCodes { get; init; } = true;

    public bool IncludeSourceSpans { get; init; } = true;
}
