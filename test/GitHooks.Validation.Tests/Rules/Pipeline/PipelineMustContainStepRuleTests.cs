using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Pipeline;

namespace GitHooks.Validation.Tests.Rules.Pipeline;

public sealed class PipelineMustContainStepRuleTests
{
    private readonly PipelineMustContainStepRule _rule = new();

    [Fact]
    public void Validate_PipelineWithNoSteps_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var pipeline = new PipelineNodeBuilder()
            .WithoutSteps()
            .WithSpan(span)
            .Build();

        _rule.Validate(pipeline, context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.PipelineMustContainStep,
            span);
    }

    [Fact]
    public void Validate_PipelineWithSteps_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript("dotnet test"))
            .Build();

        _rule.Validate(pipeline, context);

        DiagnosticAssert.Empty(diagnostics);
    }
}
