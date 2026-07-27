using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Validation.Tests;

public sealed class PipelineValidatorTests
{
    private readonly PipelineValidator _validator = new();

    [Fact]
    public void Validate_NullRoot_Throws()
    {
        var diagnostics = new DiagnosticBag();

        var exception = Assert.Throws<ArgumentNullException>(
            () => _validator.Validate(
                null!,
                diagnostics));

        Assert.Equal(
            "root",
            exception.ParamName);
    }

    [Fact]
    public void Validate_NullDiagnostics_Throws()
    {
        var pipeline = new PipelineNodeBuilder()
            .Build();

        var exception = Assert.Throws<ArgumentNullException>(
            () => _validator.Validate(
                pipeline,
                null!));

        Assert.Equal(
            "diagnostics",
            exception.ParamName);
    }

    [Fact]
    public void Validate_ValidPipeline_DoesNotReportDiagnostics()
    {
        var diagnostics = new DiagnosticBag();

        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("configuration"))
            .WithScriptStep(x => x
                .WithScript("dotnet test"))
            .Build();

        _validator.Validate(
            pipeline,
            diagnostics);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_InvalidPipeline_ReportsDiagnostics()
    {
        var document = TestSourceDocument.Default;

        var diagnostics = new DiagnosticBag();

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var pipeline = new PipelineNodeBuilder()
            .WithoutSteps()
            .WithSpan(span)
            .Build();

        _validator.Validate(
            pipeline,
            diagnostics);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.PipelineMustContainStep,
            span);
    }
}
