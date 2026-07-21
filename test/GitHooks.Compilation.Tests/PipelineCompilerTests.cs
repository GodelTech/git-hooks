using GitHooks.Compilation.Parsing;
using GitHooks.Compilation.Validation;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

using Moq;

namespace GitHooks.Compilation.Tests;

public sealed class PipelineCompilerTests
{
    private readonly Mock<IPipelineParser> _mockParser;
    private readonly Mock<IPipelineValidator> _mockValidator;

    public PipelineCompilerTests()
    {
        _mockParser = new Mock<IPipelineParser>();
        _mockValidator = new Mock<IPipelineValidator>();
    }

    [Fact]
    public void Constructor_NullParser_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PipelineCompiler(
                null!,
                _mockValidator.Object));
    }

    [Fact]
    public void Constructor_NullValidator_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PipelineCompiler(
                _mockParser.Object,
                null!));
    }

    [Fact]
    public void Compile_NullText_Throws()
    {
        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        Assert.Throws<ArgumentNullException>(
            () => compiler.Compile(
                null!,
                TestSourceDocument.Default));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Compile_EmptyOrWhitespaceText_Throws(
        string text)
    {
        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        Assert.Throws<ArgumentException>(
            () => compiler.Compile(
                text,
                TestSourceDocument.Default));
    }

    [Fact]
    public void Compile_NullDocument_Throws()
    {
        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        Assert.Throws<ArgumentNullException>(
            () => compiler.Compile(
                "test",
                null!));
    }

    [Fact]
    public void Compile_CallsParser()
    {
        _mockParser
            .Setup(x => x.Parse(It.IsAny<ParsingContext>()))
            .Returns(PipelineNode.Empty(SourceSpan.Unknown));

        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        compiler.Compile(
            "test",
            TestSourceDocument.Default);

        _mockParser.Verify(
            x => x.Parse(It.IsAny<ParsingContext>()),
            Times.Once);
    }

    [Fact]
    public void Compile_PassesParsingContextToParser()
    {
        ParsingContext? parsingContext = null;

        _mockParser
            .Setup(x => x.Parse(It.IsAny<ParsingContext>()))
            .Callback<ParsingContext>(context => parsingContext = context)
            .Returns(PipelineNode.Empty(SourceSpan.Unknown));

        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        var document = TestSourceDocument.Default;

        compiler.Compile(
            "test",
            document);

        Assert.NotNull(parsingContext);

        Assert.Equal(
            "test",
            parsingContext.Text);

        Assert.Same(
            document,
            parsingContext.Document);

        Assert.NotNull(
            parsingContext.Diagnostics);
    }

    [Fact]
    public void Compile_UsesCompilationContextDiagnostics()
    {
        ParsingContext? parsingContext = null;

        _mockParser
            .Setup(x => x.Parse(It.IsAny<ParsingContext>()))
            .Callback<ParsingContext>(context => parsingContext = context)
            .Returns(PipelineNode.Empty(SourceSpan.Unknown));

        var compilationContext = new CompilationContext();

        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        var result = compiler.Compile(
            "test",
            TestSourceDocument.Default,
            compilationContext);

        Assert.Same(
            compilationContext.Diagnostics,
            parsingContext!.Diagnostics);

        Assert.Same(
            compilationContext.Diagnostics,
            result.Diagnostics);
    }

    [Fact]
    public void Compile_ReturnsCompilationResult()
    {
        var root = PipelineNode.Empty(SourceSpan.Unknown);

        _mockParser
            .Setup(x => x.Parse(It.IsAny<ParsingContext>()))
            .Returns(root);

        var compiler = new PipelineCompiler(
            _mockParser.Object,
            _mockValidator.Object);

        var result = compiler.Compile(
            "test",
            TestSourceDocument.Default);

        Assert.Same(
            root,
            result.Root);

        Assert.NotNull(
            result.Diagnostics);
    }
}
