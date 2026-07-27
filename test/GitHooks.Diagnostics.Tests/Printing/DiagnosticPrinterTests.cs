using GitHooks.Diagnostics.Printing;

using GitHooks.Domain.Common;

using static GitHooks.Diagnostics.Tests.Printing.DiagnosticPrinterTestHelper;

namespace GitHooks.Diagnostics.Tests.Printing;

public sealed class DiagnosticPrinterTests
{
    private readonly DiagnosticPrinter _printer
        = new();

    [Fact]
    public void Print_NullDiagnosticsProvided_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => _printer.Print(null!));

        Assert.Equal(
            "diagnostics",
            exception.ParamName);
    }

    [Fact]
    public async Task Print_ErrorDiagnosticProvided_ReturnsFormattedDiagnostic()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        await VerifyDiagnosticAsync(diagnostic);
    }

    [Fact]
    public async Task Print_MultipleDiagnosticsProvided_ReturnsFormattedDiagnostics()
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

        await VerifyDiagnosticAsync(diagnostics);
    }

    [Fact]
    public async Task Print_IncludeSeverityDisabled_ReturnsDiagnosticWithoutSeverity()
    {
        var options = new DiagnosticPrinterOptions
        {
            IncludeSeverity = false
        };

        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        await VerifyDiagnosticAsync(
            diagnostic,
            options);
    }

    [Fact]
    public async Task Print_IncludeCodesDisabled_ReturnsDiagnosticWithoutCode()
    {
        var options = new DiagnosticPrinterOptions
        {
            IncludeCodes = false
        };

        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        await VerifyDiagnosticAsync(
            diagnostic,
            options);
    }

    [Fact]
    public async Task Print_IncludeSourceSpansDisabled_ReturnsDiagnosticWithoutSpan()
    {
        var options = new DiagnosticPrinterOptions
        {
            IncludeSourceSpans = false
        };

        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        await VerifyDiagnosticAsync(
            diagnostic,
            options);
    }

    [Fact]
    public async Task Print_WithRelatedLocation()
    {
        var diagnostic =
            Diagnostic.Create(
                DiagnosticDescriptors.DuplicateField,
                new SourceSpan(
                    new SourceDocument("test.yaml"),
                    new SourcePosition(5, 1),
                    new SourcePosition(5, 10)),
                [
                    DiagnosticLocation.Create(
                        new SourceSpan(
                            new SourceDocument("test.yaml"),
                            new SourcePosition(1, 1),
                            new SourcePosition(1, 10)),
                        "First declaration is here.")
                ],
                "name");

        await VerifyDiagnosticAsync(diagnostic);
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
