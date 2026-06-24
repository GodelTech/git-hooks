using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Pipeline;

namespace GitHooks.Validation.Tests.Rules.Pipeline;

public sealed class PipelineMustContainStepRuleTests
{
    private readonly PipelineMustContainStepRule _rule = new();

    [Fact]
    public void Validate_PipelineWithNoSteps_ReportsDiagnostic()
    {
        var context = TestValidationFactory.CreateContext(out var diagnostics);

        var pipeline = new PipelineNodeBuilder()
            .WithoutSteps()
            .Build();

        _rule.Validate(pipeline, context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.PipelineMustContainStep);
    }

    [Fact]
    public void Validate_PipelineWithSteps_DoesNotReportDiagnostic()
    {
        var context = TestValidationFactory.CreateContext(out var diagnostics);

        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep()
            .Build();

        _rule.Validate(pipeline, context);

        DiagnosticAssert.Empty(diagnostics);
    }
}
