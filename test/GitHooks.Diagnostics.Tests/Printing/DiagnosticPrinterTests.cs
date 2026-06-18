using GitHooks.Diagnostics.Printing;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests.Printing;

public sealed class DiagnosticPrinterTests
{
    private readonly DiagnosticPrinter _printer
        = new();

    [Fact]
    public void Print_NullDiagnosticsProvided_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _printer.Print(null!));

        Assert.Equal(
            "diagnostics",
            exception.ParamName);
    }

    [Fact]
    public void Print_ErrorDiagnosticProvided_ReturnsFormattedDiagnostic()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        var result = _printer.Print([diagnostic]);

        Assert.Equal(
            "<unknown>: error GH0001: Invalid YAML: {0}",
            result);
    }

    [Fact]
    public void Print_MultipleDiagnosticsProvided_ReturnsFormattedDiagnostics()
    {
        Diagnostic[] diagnostics =
        [
            new Diagnostic
            {
                Descriptor = DiagnosticDescriptors.InvalidYaml,
                Span = SourceSpan.Unknown
            },

            new Diagnostic
            {
                Descriptor = DiagnosticDescriptors.PipelineMustContainStep,
                Span = new SourceSpan(
                    new SourceDocument("pipeline.yaml"),
                    new SourcePosition(5, 1),
                    new SourcePosition(5, 10))
            }
        ];

        var result = _printer.Print(diagnostics);

        Assert.Equal(
            """
            <unknown>: error GH0001: Invalid YAML: {0}
            pipeline.yaml(5,1,5,10): error GH2001: Pipeline must contain at least one step.
            """,
            result);
    }

    [Fact]
    public void Print_IncludeSeverityDisabled_ReturnsDiagnosticWithoutSeverity()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter(
            new DiagnosticPrinterOptions
            {
                IncludeSeverity = false
            });

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "<unknown>: GH0001: Invalid YAML: {0}",
            result);
    }

    [Fact]
    public void Print_IncludeCodesDisabled_ReturnsDiagnosticWithoutCode()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter(
            new DiagnosticPrinterOptions
            {
                IncludeCodes = false
            });

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "<unknown>: error Invalid YAML: {0}",
            result);
    }

    [Fact]
    public void Print_IncludeSourceSpansDisabled_ReturnsDiagnosticWithoutSpan()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter(
            new DiagnosticPrinterOptions
            {
                IncludeSourceSpans = false
            });

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "error GH0001: Invalid YAML: {0}",
            result);
    }

    [Fact]
    public void Print_EmptyDiagnosticsProvided_ReturnsEmptyString()
    {
        var result = _printer.Print([]);

        Assert.Equal(
            string.Empty,
            result);
    }
}
