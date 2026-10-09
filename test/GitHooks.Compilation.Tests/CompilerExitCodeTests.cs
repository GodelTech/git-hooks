using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Tests;

public sealed class CompilerExitCodeTests
{
    [Fact]
    public void FromResult_WithoutErrors_ReturnsSuccess()
    {
        var result = new CompilationResult(
            PipelineNode.Empty(SourceSpan.Unknown),
            PipelineNode.Empty(SourceSpan.Unknown),
            new DiagnosticBag());

        var exitCode = CompilerExitCode.FromResult(result);

        Assert.Equal(
            CompilerExitCode.Success,
            exitCode);
    }

    [Fact]
    public void FromResult_WithErrors_ReturnsCompilationError()
    {
        var diagnostics = new DiagnosticBag();
        diagnostics.Report(
            Diagnostic.Create(
                new DiagnosticDescriptor
                {
                    Code = DiagnosticCode.Create(1),
                    Severity = DiagnosticSeverity.Error,
                    MessageFormat = "Message",
                },
                SourceSpan.Unknown));

        var result = new CompilationResult(
            PipelineNode.Empty(SourceSpan.Unknown),
            PipelineNode.Empty(SourceSpan.Unknown),
            diagnostics);

        var exitCode = CompilerExitCode.FromResult(result);

        Assert.Equal(
            CompilerExitCode.CompilationError,
            exitCode);
    }

    [Fact]
    public void FromResult_NullResult_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CompilerExitCode.FromResult(null!));

        Assert.Equal(
            "result",
            exception.ParamName);
    }
}
