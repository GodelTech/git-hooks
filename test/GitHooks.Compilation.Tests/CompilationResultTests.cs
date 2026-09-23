using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Tests;

public sealed class CompilationResultTests
{
    [Fact]
    public void Constructor_NullRoot_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CompilationResult(
                null!,
                PipelineNode.Empty(SourceSpan.Unknown),
                new DiagnosticBag()));

        Assert.Equal(
            "root",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullDiagnostics_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CompilationResult(
                PipelineNode.Empty(SourceSpan.Unknown),
                PipelineNode.Empty(SourceSpan.Unknown),
                null!));

        Assert.Equal(
            "diagnostics",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var root = PipelineNode.Empty(SourceSpan.Unknown);
        var boundRoot = PipelineNode.Empty(SourceSpan.Unknown);
        var diagnostics = new DiagnosticBag();

        var result = new CompilationResult(
            root,
            boundRoot,
            diagnostics);

        Assert.Same(
            root,
            result.Root);

        Assert.Same(
            boundRoot,
            result.BoundRoot);

        Assert.Same(
            diagnostics,
            result.Diagnostics);
    }

    [Fact]
    public void Result_WithoutErrorDiagnostics_HasErrorsIsFalse()
    {
        var diagnostics = new DiagnosticBag();
        diagnostics.Report(CreateDiagnostic(DiagnosticSeverity.Warning, 1));

        var result = new CompilationResult(
            PipelineNode.Empty(SourceSpan.Unknown),
            diagnostics);

        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Result_WithErrorDiagnostics_HasErrorsIsTrue()
    {
        var diagnostics = new DiagnosticBag();
        diagnostics.Report(CreateDiagnostic(DiagnosticSeverity.Warning, 1));
        diagnostics.Report(CreateDiagnostic(DiagnosticSeverity.Error, 2));

        var result = new CompilationResult(
            PipelineNode.Empty(SourceSpan.Unknown),
            diagnostics);

        Assert.True(result.HasErrors);
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticSeverity severity,
        int code)
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor
            {
                Code = DiagnosticCode.Create(code),
                Severity = severity,
                MessageFormat = "Message",
            },
            SourceSpan.Unknown);
    }
}
