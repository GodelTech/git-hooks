using System.Diagnostics;
using System.Globalization;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics;

[DebuggerDisplay("{Code}: {Message}")]
public sealed record Diagnostic
{
    public required DiagnosticDescriptor Descriptor { get; init; }

    public required SourceSpan Span { get; init; }

    public IReadOnlyList<object?> Arguments { get; init; }
        = [];

    public IReadOnlyList<DiagnosticLocation> RelatedLocations { get; init; }
        = [];

    public DiagnosticCode Code
        => Descriptor.Code;

    public DiagnosticSeverity Severity
        => Descriptor.Severity;

    public string Message
        => Arguments.Count == 0
            ? Descriptor.MessageFormat
            : string.Format(
                CultureInfo.InvariantCulture,
                Descriptor.MessageFormat,
                [.. Arguments]);

    public static Diagnostic Create(
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        return Create(
            descriptor,
            span,
            [],
            arguments);
    }

    public static Diagnostic Create(
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        IReadOnlyList<DiagnosticLocation> relatedLocations,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(relatedLocations);
        ArgumentNullException.ThrowIfNull(arguments);

        return new Diagnostic
        {
            Descriptor = descriptor,
            Span = span,
            Arguments = arguments,
            RelatedLocations = relatedLocations
        };
    }
}
