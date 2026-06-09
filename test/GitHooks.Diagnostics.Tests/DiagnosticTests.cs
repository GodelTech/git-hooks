using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticTests
{
    [Fact]
    public void Code_ReturnsDescriptorCode()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml.Code,
            diagnostic.Code);
    }

    [Fact]
    public void Severity_ReturnsDescriptorSeverity()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml.Severity,
            diagnostic.Severity);
    }

    [Fact]
    public void Message_WhenDiagnosticHasNoArguments_ReturnsDescriptorMessageFormat()
    {
        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown
        };

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml.MessageFormat,
            diagnostic.Message);
    }

    [Fact]
    public void Message_WhenDiagnosticHasArguments_FormatsMessage()
    {
        var descriptor = new DiagnosticDescriptor
        {
            Code = DiagnosticCode.InvalidYaml,
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Expected '{0}' but found '{1}'."
        };

        var diagnostic = new Diagnostic
        {
            Descriptor = descriptor,
            Span = SourceSpan.Unknown,
            Arguments = ["Scalar", "Mapping"]
        };

        Assert.Equal(
            "Expected 'Scalar' but found 'Mapping'.",
            diagnostic.Message);
    }
}
