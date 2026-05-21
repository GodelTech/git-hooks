using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics;

public sealed record Diagnostic
{
    public required DiagnosticCode Code { get; init; }

    public required string Message { get; init; }

    public required DiagnosticSeverity Severity { get; init; }

    public required SourceSpan Span { get; init; }
}
