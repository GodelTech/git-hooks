using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterNameMustBeValidIdentifierRuleTests
{
    private readonly ParameterNameMustBeValidIdentifierRule _rule = new();

    [Fact]
    public void Validate_NameIsNotStringLiteral_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName(x => x
                .WithKey("name")
                .WithIntegerValue(v => v.WithValue(123)))
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
    public void Validate_NameIsEmptyOrWhitespace_DoesNotReportDiagnostic(
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

    [Theory]
    [InlineData("configuration")]
    [InlineData("_config")]
    [InlineData("config123")]
    [InlineData("_123")]
    public void Validate_NameIsValidIdentifier_DoesNotReportDiagnostic(
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

    [Theory]
    [InlineData("1config")]
    [InlineData("my-param")]
    [InlineData("my param")]
    [InlineData("config!")]
    public void Validate_NameIsInvalidIdentifier_ReportsDiagnostic(
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
            DiagnosticDescriptors.ParameterNameMustBeValidIdentifier,
            span,
            name);
    }
}
