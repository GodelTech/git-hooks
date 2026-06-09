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

        bag.Report(DiagnosticDescriptors.InvalidYaml, SourceSpan.Unknown);

        var result = Assert.Single(bag.Diagnostics);

        Assert.Same(
            DiagnosticDescriptors.InvalidYaml,
            result.Descriptor);

        Assert.Equal(
            SourceSpan.Unknown,
            result.Span);
    }
}
