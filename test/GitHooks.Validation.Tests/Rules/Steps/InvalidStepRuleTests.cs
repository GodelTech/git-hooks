using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Steps;

namespace GitHooks.Validation.Tests.Rules.Steps;

public sealed class InvalidStepRuleTests
{
    private readonly InvalidStepRule _rule = new();

    [Fact]
    public void Validate_InvalidStepWithoutFields_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var step = new InvalidStepNodeBuilder()
            .WithSpan(span)
            .Build();

        _rule.Validate(step, context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.InvalidStep,
            span);
    }

    [Fact]
    public void Validate_InvalidStepWithFields_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 2, 1, 2, 20);

        var step = new InvalidStepNodeBuilder()
            .WithField("unknownField", "value")
            .WithSpan(span)
            .Build();

        _rule.Validate(step, context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.InvalidStep,
            span);
    }
}
