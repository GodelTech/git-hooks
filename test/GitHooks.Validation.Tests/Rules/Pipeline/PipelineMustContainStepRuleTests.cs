using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders;
using GitHooks.Validation.Rules.Pipeline;

namespace GitHooks.Validation.Tests.Rules.Pipeline;

public sealed class PipelineMustContainStepRuleTests
{
    private readonly PipelineMustContainStepRule _rule = new();

    [Fact]
    public void Validate_WhenPipelineContainsNoSteps_ShouldReportDiagnostic()
    {
        var diagnosticBag = new DiagnosticBag();

        var pipeline = new PipelineNodeBuilder()
            .WithoutSteps()
            .Build();

        var expectedDiagnostic = new Diagnostic
        {
            Descriptor = DiagnosticDescriptors.PipelineMustContainStep,
            Span = pipeline.Span
        };

        _rule.Validate(pipeline, diagnosticBag);

        var diagnostic = Assert.Single(diagnosticBag.Diagnostics);

        Assert.Equal(
            expectedDiagnostic,
            diagnostic);
    }

    [Fact]
    public void Validate_WhenPipelineContainsSteps_ShouldNotReportDiagnostic()
    {
        var diagnosticBag = new DiagnosticBag();

        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep()
            .Build();

        _rule.Validate(pipeline, diagnosticBag);

        Assert.Empty(diagnosticBag.Diagnostics);
    }
}
