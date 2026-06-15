using GitHooks.Diagnostics;

namespace GitHooks.Testing.Diagnostics;

public static class DiagnosticAssert
{
    public static Diagnostic Single(
        IReadOnlyList<Diagnostic> diagnostics,
        DiagnosticDescriptor descriptor,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(descriptor);

        var diagnostic = Assert.Single(diagnostics);

        Matches(diagnostic, descriptor, arguments);

        return diagnostic;
    }

    public static void Matches(
        Diagnostic diagnostic,
        DiagnosticDescriptor descriptor,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        ArgumentNullException.ThrowIfNull(descriptor);

        Assert.Equal(
            descriptor,
            diagnostic.Descriptor);

        Assert.Equal(
            arguments,
            diagnostic.Arguments);
    }
}
