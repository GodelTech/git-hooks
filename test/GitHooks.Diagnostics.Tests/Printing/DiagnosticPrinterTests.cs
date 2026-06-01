using GitHooks.Diagnostics.Printing;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests.Printing;

public sealed class DiagnosticPrinterTests
{
    [Fact]
    public void Print_ErrorDiagnosticProvided_ReturnsFormattedDiagnostic()
    {
        var diagnostic = new Diagnostic
        {
            Code = DiagnosticCode.InvalidYaml,
            Message = "Invalid YAML syntax.",
            Severity = DiagnosticSeverity.Error,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter();

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "Error GH0001: Invalid YAML syntax. @ <unknown>",
            result);
    }

    [Fact]
    public void Print_MultipleDiagnosticsProvided_ReturnsFormattedDiagnostics()
    {
        Diagnostic[] diagnostics =
        [
            new Diagnostic
            {
                Code = DiagnosticCode.InvalidYaml,
                Message = "Invalid YAML syntax.",
                Severity = DiagnosticSeverity.Error,
                Span = SourceSpan.Unknown
            },

            new Diagnostic
            {
                Code = DiagnosticCode.UnknownStepType,
                Message = "Unknown step type.",
                Severity = DiagnosticSeverity.Warning,
                Span = new SourceSpan(
                    new SourcePosition(5, 1),
                    new SourcePosition(5, 10))
            }
        ];

        var printer = new DiagnosticPrinter();

        var result = printer.Print(diagnostics);

        Assert.Equal(
            """
            Error GH0001: Invalid YAML syntax. @ <unknown>
            Warning GH0005: Unknown step type. @ (5:1-5:10)
            """,
            result);
    }

    [Fact]
    public void Print_IncludeSeverityDisabled_ReturnsDiagnosticWithoutSeverity()
    {
        var diagnostic = new Diagnostic
        {
            Code = DiagnosticCode.InvalidYaml,
            Message = "Invalid YAML syntax.",
            Severity = DiagnosticSeverity.Error,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter(
            new DiagnosticPrinterOptions
            {
                IncludeSeverity = false
            });

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "GH0001: Invalid YAML syntax. @ <unknown>",
            result);
    }

    [Fact]
    public void Print_IncludeCodesDisabled_ReturnsDiagnosticWithoutCode()
    {
        var diagnostic = new Diagnostic
        {
            Code = DiagnosticCode.InvalidYaml,
            Message = "Invalid YAML syntax.",
            Severity = DiagnosticSeverity.Error,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter(
            new DiagnosticPrinterOptions
            {
                IncludeCodes = false
            });

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "Error Invalid YAML syntax. @ <unknown>",
            result);
    }

    [Fact]
    public void Print_IncludeSourceSpansDisabled_ReturnsDiagnosticWithoutSpan()
    {
        var diagnostic = new Diagnostic
        {
            Code = DiagnosticCode.InvalidYaml,
            Message = "Invalid YAML syntax.",
            Severity = DiagnosticSeverity.Error,
            Span = SourceSpan.Unknown
        };

        var printer = new DiagnosticPrinter(
            new DiagnosticPrinterOptions
            {
                IncludeSourceSpans = false
            });

        var result = printer.Print([diagnostic]);

        Assert.Equal(
            "Error GH0001: Invalid YAML syntax.",
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
