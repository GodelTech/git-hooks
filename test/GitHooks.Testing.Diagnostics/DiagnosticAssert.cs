using GitHooks.Diagnostics;

namespace GitHooks.Testing.Diagnostics;

public static class DiagnosticAssert
{
    public static Diagnostic Single(
        IReadOnlyList<Diagnostic> diagnostics,
        DiagnosticDescriptor descriptor)
    {
        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal(
            descriptor,
            diagnostic.Descriptor);

        return diagnostic;
    }
}
