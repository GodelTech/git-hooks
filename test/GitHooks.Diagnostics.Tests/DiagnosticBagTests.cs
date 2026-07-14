using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticBagTests
{
    private readonly DiagnosticBag _diagnosticBag
        = new();

    [Fact]
    public void Diagnostics_IsEmptyByDefault()
    {
        DiagnosticAssert.Empty(_diagnosticBag);
    }

    [Fact]
    public void Report_AddsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        _diagnosticBag.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.InvalidYaml,
                span));

        DiagnosticAssert.Single(
            _diagnosticBag,
            DiagnosticDescriptors.InvalidYaml,
            span);
    }
}
