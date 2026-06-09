namespace GitHooks.Diagnostics;

public sealed record DiagnosticDescriptor
{
    public required DiagnosticCode Code { get; init; }

    public required DiagnosticSeverity Severity { get; init; }

    public required string MessageFormat { get; init; }
}
