using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Steps;

namespace GitHooks.Validation.Tests.Rules.Steps;

public sealed class ScriptFieldMustNotBeEmptyRuleTests
{
    private readonly ScriptFieldMustNotBeEmptyRule _rule = new();

    [Fact]
    public void Validate_ScriptIsNotStringLiteral_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var step = new ScriptStepNodeBuilder()
            .WithScript(x => x
                .WithKey("script")
                .WithIntegerValue(v => v.WithValue(123)))
            .Build();

        _rule.Validate(
            step,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("dotnet test")]
    [InlineData(" echo hello ")]
    public void Validate_ScriptIsNotEmpty_DoesNotReportDiagnostic(
        string script)
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var step = new ScriptStepNodeBuilder()
            .WithScript(script)
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
    public void Validate_ScriptIsEmpty_ReportsDiagnostic(
        string script)
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var step = new ScriptStepNodeBuilder()
            .WithScript(x => x
                .WithKey("script")
                .WithValue(script)
                .WithSpan(span))
            .Build();

        _rule.Validate(
            step,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ScriptFieldMustNotBeEmpty,
            span);
    }
}
