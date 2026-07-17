using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Tests;

public sealed class CompilationResultTests
{
    [Fact]
    public void Constructor_NullRoot_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CompilationResult(
                null!,
                new DiagnosticBag()));
    }

    [Fact]
    public void Constructor_NullDiagnostics_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CompilationResult(
                PipelineNode.Empty(SourceSpan.Unknown),
                null!));
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var root = PipelineNode.Empty(SourceSpan.Unknown);
        var diagnostics = new DiagnosticBag();

        var result = new CompilationResult(
            root,
            diagnostics);

        Assert.Same(
            root,
            result.Root);

        Assert.Same(
            diagnostics,
            result.Diagnostics);
    }
}
