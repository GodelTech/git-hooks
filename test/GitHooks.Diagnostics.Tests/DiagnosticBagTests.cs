using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticBagTests
{
    [Fact]
    public void Diagnostics_IsEmptyByDefault()
    {
        var diagnostics = new DiagnosticBag();

        Assert.Empty(diagnostics.Diagnostics);
    }

    [Fact]
    public void Report_AddsDiagnostic()
    {
        var diagnostics = new DiagnosticBag();

        diagnostics.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.InvalidYaml,
                SourceSpan.Unknown));

        var result = Assert.Single(diagnostics.Diagnostics);

        Assert.Same(
            DiagnosticDescriptors.InvalidYaml,
            result.Descriptor);

        Assert.Equal(
            SourceSpan.Unknown,
            result.Span);
    }
}
