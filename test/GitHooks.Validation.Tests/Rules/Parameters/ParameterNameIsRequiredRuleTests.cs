using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterNameIsRequiredRuleTests
{
    private readonly ParameterNameIsRequiredRule _rule = new();

    [Theory]
    [InlineData("configuration")]
    [InlineData(" configuration ")]
    public void Validate_NameIsValidString_DoesNotReportDiagnostic(
        string name)
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName(name)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_NameIsNotStringLiteral_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var parameter = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithIntegerValue(v => v.WithValue(123)))
            .WithSpan(span)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterNameIsRequired,
            span);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validate_NameIsEmpty_ReportsDiagnostic(
        string name)
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var parameter = new ParameterNodeBuilder()
            .WithName(name)
            .WithSpan(span)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterNameIsRequired,
            span);
    }
}
