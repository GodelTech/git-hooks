using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics;

public sealed record DiagnosticLocation
{
    public required string Message { get; init; }

    public required SourceSpan Span { get; init; }

    public static DiagnosticLocation Create(
        SourceSpan span,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new DiagnosticLocation
        {
            Span = span,
            Message = message
        };
    }
}
