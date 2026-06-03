using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticBagTests
{
    [Fact]
    public void Diagnostics_IsEmptyByDefault()
    {
        var bag = new DiagnosticBag();

        Assert.Empty(bag.Diagnostics);
    }

    [Fact]
    public void Report_AddsDiagnostic()
    {
        var bag = new DiagnosticBag();

        var diagnostic =
            new Diagnostic
            {
                Code = new DiagnosticCode("GH0001"),
                Message = "Test",
                Severity = DiagnosticSeverity.Error,
                Span = SourceSpan.Unknown
            };

        bag.Report(diagnostic);

        var result = Assert.Single(bag.Diagnostics);

        Assert.Same(
            diagnostic,
            result);
    }
}
