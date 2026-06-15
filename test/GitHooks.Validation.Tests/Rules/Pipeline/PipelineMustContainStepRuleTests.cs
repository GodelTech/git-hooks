using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Pipeline;

namespace GitHooks.Validation.Tests.Rules.Pipeline;

public sealed class PipelineMustContainStepRuleTests
{
    private readonly PipelineMustContainStepRule _rule = new();

    [Fact]
    public void Validate_WhenPipelineContainsNoSteps_ShouldReportDiagnostic()
    {
        var diagnostics = new DiagnosticBag();

        var pipeline = new PipelineNodeBuilder()
            .WithoutSteps()
            .Build();

        _rule.Validate(pipeline, diagnostics);

        DiagnosticAssert.Single(
            diagnostics.Diagnostics,
            DiagnosticDescriptors.PipelineMustContainStep);
    }

    [Fact]
    public void Validate_WhenPipelineContainsSteps_ShouldNotReportDiagnostic()
    {
        var diagnostics = new DiagnosticBag();

        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep()
            .Build();

        _rule.Validate(pipeline, diagnostics);

        Assert.Empty(diagnostics.Diagnostics);
    }
}
