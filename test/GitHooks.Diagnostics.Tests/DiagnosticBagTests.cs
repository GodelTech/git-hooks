using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticBagTests
{
    private readonly DiagnosticBag _diagnosticBag
        = new();

    [Fact]
    public void Diagnostics_IsEmptyByDefault()
    {
        Assert.Empty(_diagnosticBag.Diagnostics);
    }

    [Fact]
    public void Report_AddsDiagnostic()
    {
        _diagnosticBag.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.InvalidYaml,
                SourceSpan.Unknown));

        var result = Assert.Single(_diagnosticBag.Diagnostics);

        Assert.Same(
            DiagnosticDescriptors.InvalidYaml,
            result.Descriptor);

        Assert.Equal(
            SourceSpan.Unknown,
            result.Span);
    }
}
