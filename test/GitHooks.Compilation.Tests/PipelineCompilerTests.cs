using GitHooks.Compilation.Validation;
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
    public void Constructor_NullAstCompiler_Throws()
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
    public void Constructor_NullValidator_Throws()
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
    public void Compile_NullText_Throws()
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
    public void Compile_EmptyOrWhitespaceText_Throws(
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
    public void Compile_NullDocument_Throws()
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
}
