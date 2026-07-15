using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterNameMustBeUniqueRuleTests
{
    private readonly ParameterNameMustBeUniqueRule _rule = new();

    [Fact]
    public void Validate_NameIsMissing_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName(string.Empty)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_ParameterIsNotInSymbolTable_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_FirstParameter_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        context.Symbols.AddParameter(parameter);

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_DuplicateParameterName_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var firstSpan = TestSourceSpan.Create(document, 1, 1, 1, 13);
        var secondSpan = TestSourceSpan.Create(document, 2, 1, 2, 13);

        var first = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithValue("configuration")
                .WithSpan(firstSpan))
            .Build();

        var duplicate = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithValue("configuration")
                .WithSpan(secondSpan))
            .Build();

        context.Symbols.AddParameter(first);

        _rule.Validate(
            duplicate,
            context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            diagnostics,
            DiagnosticDescriptors.ParameterNameMustBeUnique,
            secondSpan,
            "configuration");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstSpan);
    }
}
