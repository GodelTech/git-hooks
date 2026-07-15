using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterDisplayNameMustNotBeEmptyRuleTests
{
    private readonly ParameterDisplayNameMustNotBeEmptyRule _rule = new();

    [Fact]
    public void Validate_DisplayNameIsNull_DoesNotReportDiagnostic()
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
    public void Validate_DisplayNameIsNotStringLiteral_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithDisplayName(x => x
                .WithKey("displayName")
                .WithIntegerValue(v => v.WithValue(123)))
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("Configuration")]
    [InlineData(" Configuration ")]
    public void Validate_DisplayNameIsNotEmpty_DoesNotReportDiagnostic(
        string displayName)
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithDisplayName(displayName)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validate_DisplayNameIsEmpty_ReportsDiagnostic(
        string displayName)
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithDisplayName(x => x
                .WithKey("displayName")
                .WithValue(displayName)
                .WithSpan(span))
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterDisplayNameMustNotBeEmpty,
            span,
            "configuration");
    }
}
