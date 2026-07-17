using GitHooks.Domain.Common;
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

    [Fact]
    public void HasErrors_WithErrorDiagnostic_ReturnsTrue()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = new DiagnosticDescriptor
            {
                Code = DiagnosticCode.Create(1),
                Severity = DiagnosticSeverity.Error,
                MessageFormat = "Error"
            },
            Span = SourceSpan.Unknown
        };

        _diagnosticBag.Report(diagnostic);

        Assert.True(_diagnosticBag.HasErrors);
    }

    [Fact]
    public void HasErrors_WithoutErrorDiagnostic_ReturnsFalse()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = new DiagnosticDescriptor
            {
                Code = DiagnosticCode.Create(2),
                Severity = DiagnosticSeverity.Warning,
                MessageFormat = "Warning"
            },
            Span = SourceSpan.Unknown
        };

        _diagnosticBag.Report(diagnostic);

        Assert.False(_diagnosticBag.HasErrors);
    }
}
