using GitHooks.Diagnostics;

namespace GitHooks.Validation.Tests;

internal static class TestValidationContextFactory
{
    public static ValidationContext Create(
        out DiagnosticBag diagnostics)
    {
        diagnostics = new DiagnosticBag();

        return new ValidationContext(
            diagnostics);
    }
}
