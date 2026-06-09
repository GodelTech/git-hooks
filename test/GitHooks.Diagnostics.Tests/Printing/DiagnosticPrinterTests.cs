using GitHooks.Diagnostics.Printing;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests.Printing;

public sealed class DiagnosticPrinterTests
{
    [Fact]
    public void Print_NullDiagnosticsProvided_Throws()
    {
        var printer = new DiagnosticPrinter();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => printer.Print(null!));

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

        var printer = new DiagnosticPrinter();

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "Error GH0001: Invalid YAML: {0} @ <unknown>",
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

        var printer = new DiagnosticPrinter();

        var result = printer.Print(diagnostics);

        Assert.Equal(
            """
            Error GH0001: Invalid YAML: {0} @ <unknown>
            Error GH1001: Pipeline must contain at least one step. @ (5:1-5:10)
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
            "GH0001: Invalid YAML: {0} @ <unknown>",
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
            "Error Invalid YAML: {0} @ <unknown>",
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
            "Error GH0001: Invalid YAML: {0}",
            result);
    }

    [Fact]
    public void Print_EmptyDiagnosticsProvided_ReturnsEmptyString()
    {
        var printer = new DiagnosticPrinter();

        var result = printer.Print([]);

        Assert.Equal(
            string.Empty,
            result);
    }
}
