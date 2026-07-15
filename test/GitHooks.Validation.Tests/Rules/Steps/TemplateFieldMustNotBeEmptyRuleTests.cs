using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Steps;

namespace GitHooks.Validation.Tests.Rules.Steps;

public sealed class TemplateFieldMustNotBeEmptyRuleTests
{
    private readonly TemplateFieldMustNotBeEmptyRule _rule = new();

    [Fact]
    public void Validate_TemplateIsNotStringLiteral_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var step = new TemplateStepNodeBuilder()
            .WithTemplate(x => x
                .WithKey("template")
                .WithIntegerValue(v => v.WithValue(123)))
            .Build();

        _rule.Validate(
            step,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("build.yml")]
    [InlineData(" templates/build.yml ")]
    public void Validate_TemplateIsNotEmpty_DoesNotReportDiagnostic(
        string template)
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var step = new TemplateStepNodeBuilder()
            .WithTemplate(template)
            .Build();

        _rule.Validate(
            step,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validate_TemplateIsEmpty_ReportsDiagnostic(
        string template)
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var step = new TemplateStepNodeBuilder()
            .WithTemplate(x => x
                .WithKey("template")
                .WithValue(template)
                .WithSpan(span))
            .Build();

        _rule.Validate(
            step,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.TemplateFieldMustNotBeEmpty,
            span);
    }
}
