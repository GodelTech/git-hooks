using GitHooks.Diagnostics;

namespace GitHooks.Validation.Tests;

internal static class TestValidationFactory
{
    public static ValidationContext CreateContext(
        out DiagnosticBag diagnostics)
    {
        diagnostics = new DiagnosticBag();

        return new ValidationContext(
            diagnostics);
    }
}
