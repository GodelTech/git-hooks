using GitHooks.Domain.Common;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticTests
{
    [Fact]
    public void Create_WithoutArguments_ReturnsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result = Diagnostic.Create(
            DiagnosticDescriptors.InvalidYaml,
            span);

        DiagnosticAssert.Matches(
            result,
            DiagnosticDescriptors.InvalidYaml,
            span);
    }

    [Fact]
    public void Create_WithArguments_ReturnsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result = Diagnostic.Create(
            DiagnosticDescriptors.DuplicateField,
            span,
            "configuration");

        DiagnosticAssert.Matches(
            result,
            DiagnosticDescriptors.DuplicateField,
            span,
            "configuration");
    }

    [Fact]
    public void Message_WithoutArguments_ReturnsDescriptorMessage()
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.InvalidYaml,
            SourceSpan.Unknown);

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml.MessageFormat,
            diagnostic.Message);
    }

    [Fact]
    public void Code_ReturnsDescriptorCode()
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.InvalidYaml,
            SourceSpan.Unknown);

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml.Code,
            diagnostic.Code);
    }

    [Fact]
    public void Severity_ReturnsDescriptorSeverity()
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.InvalidYaml,
            SourceSpan.Unknown);

        Assert.Equal(
            DiagnosticDescriptors.InvalidYaml.Severity,
            diagnostic.Severity);
    }

    [Fact]
    public void Message_WithArguments_ReturnsFormattedMessage()
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.DuplicateField,
            SourceSpan.Unknown,
            "configuration");

        Assert.Equal(
            "Duplicate 'configuration' field.",
            diagnostic.Message);
    }

    [Fact]
    public void RelatedLocations_DefaultsToEmptyCollection()
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.InvalidYaml,
            SourceSpan.Unknown);

        Assert.Empty(
            diagnostic.RelatedLocations);
    }

    [Fact]
    public void RelatedLocations_CanBeAssigned()
    {
        var location = DiagnosticLocation.Create(
            SourceSpan.Unknown,
            "First declaration is here.");

        var diagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.InvalidYaml,
            Span = SourceSpan.Unknown,
            RelatedLocations = [location]
        };

        var result = Assert.Single(
            diagnostic.RelatedLocations);

        Assert.Equal(
            location,
            result);
    }
}
