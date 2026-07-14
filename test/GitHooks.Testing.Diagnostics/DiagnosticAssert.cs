using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

namespace GitHooks.Testing.Diagnostics;

public static class DiagnosticAssert
{
    public static void Empty(
        DiagnosticBag diagnostics)
    {
        Assert.NotNull(diagnostics);

        Empty(diagnostics.Diagnostics);
    }

    public static void Empty(
        IReadOnlyList<Diagnostic> diagnostics)
    {
        Assert.NotNull(diagnostics);

        Assert.Empty(diagnostics);
    }

    public static Diagnostic Single(
        DiagnosticBag diagnostics,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.NotNull(diagnostics);

        return Single(
            diagnostics.Diagnostics,
            descriptor,
            span,
            arguments);
    }

    public static Diagnostic Single(
        IReadOnlyList<Diagnostic> diagnostics,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.NotNull(diagnostics);
        Assert.NotNull(descriptor);

        var diagnostic = Assert.Single(diagnostics);

        Matches(
            diagnostic,
            descriptor,
            span,
            arguments);

        return diagnostic;
    }

    public static Diagnostic SingleWithRelatedLocations(
        DiagnosticBag diagnostics,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.NotNull(diagnostics);

        return SingleWithRelatedLocations(
            diagnostics.Diagnostics,
            descriptor,
            span,
            arguments);
    }

    public static Diagnostic SingleWithRelatedLocations(
        IReadOnlyList<Diagnostic> diagnostics,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.NotNull(diagnostics);
        Assert.NotNull(descriptor);

        var diagnostic = Assert.Single(diagnostics);

        MatchesWithRelatedLocations(
            diagnostic,
            descriptor,
            span,
            arguments);

        return diagnostic;
    }

    public static void Matches(
        Diagnostic diagnostic,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.NotNull(diagnostic);
        Assert.NotNull(descriptor);

        MatchesCore(
            diagnostic,
            descriptor,
            span,
            arguments);

        Assert.Empty(diagnostic.RelatedLocations);
    }

    public static void MatchesWithRelatedLocations(
        Diagnostic diagnostic,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.NotNull(diagnostic);
        Assert.NotNull(descriptor);

        MatchesCore(
            diagnostic,
            descriptor,
            span,
            arguments);

        Assert.NotEmpty(diagnostic.RelatedLocations);
    }

    public static DiagnosticLocation SingleRelatedLocation(
        Diagnostic diagnostic,
        string message,
        SourceSpan span)
    {
        Assert.NotNull(diagnostic);

        var relatedLocation = Assert.Single(diagnostic.RelatedLocations);

        Assert.Equal(
            message,
            relatedLocation.Message);

        SourceSpanAssert.Equal(
            span,
            relatedLocation.Span);

        return relatedLocation;
    }

    private static void MatchesCore(
        Diagnostic diagnostic,
        DiagnosticDescriptor descriptor,
        SourceSpan span,
        params object?[] arguments)
    {
        Assert.Equal(
            descriptor,
            diagnostic.Descriptor);

        SourceSpanAssert.Equal(
            span,
            diagnostic.Span);

        Assert.Equal(
            arguments,
            [.. diagnostic.Arguments]);
    }
}
