using GitHooks.Compilation.Validation;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

using Moq;

namespace GitHooks.Compilation.Tests;

public sealed class PipelineCompilerTests
{
    private readonly Mock<IPipelineAstCompiler> _mockAstCompiler;
    private readonly Mock<IPipelineValidator> _mockValidator;

    public PipelineCompilerTests()
    {
        _mockAstCompiler = new Mock<IPipelineAstCompiler>();
        _mockValidator = new Mock<IPipelineValidator>();
    }

    [Fact]
    public void Constructor_NullAstCompiler_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineCompiler(
                null!,
                _mockValidator.Object));

        Assert.Equal(
            "astCompiler",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullValidator_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineCompiler(
                _mockAstCompiler.Object,
                null!));

        Assert.Equal(
            "validator",
            exception.ParamName);
    }

    [Fact]
    public void Compile_NullText_ThrowsArgumentNullException()
    {
        var compiler = new PipelineCompiler(
            _mockAstCompiler.Object,
            _mockValidator.Object);

        var exception = Assert.Throws<ArgumentNullException>(
            () => compiler.Compile(
                null!,
                TestSourceDocument.Default));

        Assert.Equal(
            "text",
            exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Compile_EmptyOrWhitespaceText_ThrowsArgumentException(
        string text)
    {
        var compiler = new PipelineCompiler(
            _mockAstCompiler.Object,
            _mockValidator.Object);

        var exception = Assert.Throws<ArgumentException>(
            () => compiler.Compile(
                text,
                TestSourceDocument.Default));

        Assert.Equal(
            "text",
            exception.ParamName);
    }

    [Fact]
    public void Compile_NullDocument_ThrowsArgumentNullException()
    {
        var compiler = new PipelineCompiler(
            _mockAstCompiler.Object,
            _mockValidator.Object);

        var exception = Assert.Throws<ArgumentNullException>(
            () => compiler.Compile(
                "test",
                null!));

        Assert.Equal(
            "document",
            exception.ParamName);
    }

    [Fact]
    public void Compile_ValidPipeline_ReturnsResultWithoutErrorsAndInvokesDependenciesOnce()
    {
        var expectedRoot = PipelineNode.Empty(SourceSpan.Unknown);

        _mockAstCompiler
            .Setup(static astCompiler => astCompiler.Compile(
                It.IsAny<SourceContent>(),
                It.IsAny<CompilationContext>()))
            .Returns(expectedRoot);

        var compiler = new PipelineCompiler(
            _mockAstCompiler.Object,
            _mockValidator.Object);

        var result = compiler.Compile(
            "name: test",
            TestSourceDocument.Default);

        Assert.Same(
            expectedRoot,
            result.Root);

        Assert.False(
            result.Diagnostics.HasErrors);

        _mockAstCompiler.Verify(
            astCompiler => astCompiler.Compile(
                It.IsAny<SourceContent>(),
                It.IsAny<CompilationContext>()),
            Times.Once);

        _mockValidator.Verify(
            validator => validator.Validate(
                expectedRoot,
                It.IsAny<DiagnosticBag>()),
            Times.Once);
    }

    [Fact]
    public void Compile_ValidatorReportsError_ReturnsResultWithErrors()
    {
        var expectedRoot = PipelineNode.Empty(SourceSpan.Unknown);

        _mockAstCompiler
            .Setup(static astCompiler => astCompiler.Compile(
                It.IsAny<SourceContent>(),
                It.IsAny<CompilationContext>()))
            .Returns(expectedRoot);

        _mockValidator
            .Setup(static validator => validator.Validate(
                It.IsAny<PipelineNode>(),
                It.IsAny<DiagnosticBag>()))
            .Callback(static (PipelineNode _, DiagnosticBag diagnostics) =>
                diagnostics.Report(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidYaml,
                        SourceSpan.Unknown,
                        "test")));

        var compiler = new PipelineCompiler(
            _mockAstCompiler.Object,
            _mockValidator.Object);

        var result = compiler.Compile(
            "name: test",
            TestSourceDocument.Default);

        Assert.True(
            result.Diagnostics.HasErrors);

        Assert.Single(
            result.Diagnostics.Diagnostics);

        _mockAstCompiler.Verify(
            astCompiler => astCompiler.Compile(
                It.IsAny<SourceContent>(),
                It.IsAny<CompilationContext>()),
            Times.Once);

        _mockValidator.Verify(
            validator => validator.Validate(
                expectedRoot,
                It.IsAny<DiagnosticBag>()),
            Times.Once);
    }
}
