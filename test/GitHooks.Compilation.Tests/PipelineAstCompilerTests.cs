using GitHooks.Compilation.Expansion;
using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

using Moq;

namespace GitHooks.Compilation.Tests;

public sealed class PipelineAstCompilerTests
{
    private readonly Mock<IPipelineParser> _mockParser;
    private readonly Mock<ITemplateExpander> _mockExpander;

    public PipelineAstCompilerTests()
    {
        _mockParser = new Mock<IPipelineParser>();
        _mockExpander = new Mock<ITemplateExpander>();
    }

    [Fact]
    public void Constructor_NullParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineAstCompiler(
                null!,
                _mockExpander.Object));

        Assert.Equal(
            "parser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullExpander_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineAstCompiler(
                _mockParser.Object,
                null!));

        Assert.Equal(
            "expander",
            exception.ParamName);
    }

    [Fact]
    public void Compile_NullSource_Throws()
    {
        var compiler = new PipelineAstCompiler(
            _mockParser.Object,
            _mockExpander.Object);

        var exception = Assert.Throws<ArgumentNullException>(
            () => compiler.Compile(
                null!,
                new CompilationContext()));

        Assert.Equal(
            "source",
            exception.ParamName);
    }

    [Fact]
    public void Compile_NullContext_Throws()
    {
        var compiler = new PipelineAstCompiler(
            _mockParser.Object,
            _mockExpander.Object);

        var source = new SourceContent(
            "name: test",
            TestSourceDocument.Default);

        var exception = Assert.Throws<ArgumentNullException>(
            () => compiler.Compile(
                source,
                null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void Compile_ValidSource_ParsesThenExpandsAndReturnsExpandedResult()
    {
        var parsedRoot = PipelineNode.Empty(SourceSpan.Unknown);
        var expandedRoot = PipelineNode.Empty(SourceSpan.Unknown);

        _mockParser
            .Setup(static parser => parser.Parse(
                It.IsAny<ParsingContext>(),
                It.IsAny<DiagnosticBag>()))
            .Returns(parsedRoot);

        _mockExpander
            .Setup(static expander => expander.Expand(
                It.IsAny<PipelineNode>(),
                It.IsAny<ExpansionContext>(),
                It.IsAny<DiagnosticBag>()))
            .Returns(expandedRoot);

        var compiler = new PipelineAstCompiler(
            _mockParser.Object,
            _mockExpander.Object);

        var context = new CompilationContext();

        var source = new SourceContent(
            "name: test",
            TestSourceDocument.Default);

        var result = compiler.Compile(
            source,
            context);

        Assert.Same(
            expandedRoot,
            result);

        _mockParser.Verify(
            parser => parser.Parse(
                It.IsAny<ParsingContext>(),
                context.Diagnostics),
            Times.Once);

        _mockExpander.Verify(
            expander => expander.Expand(
                parsedRoot,
                It.IsAny<ExpansionContext>(),
                context.Diagnostics),
            Times.Once);
    }

    [Fact]
    public void Compile_ParserReportsError_StillInvokesExpanderAndPropagatesDiagnostics()
    {
        var parsedRoot = PipelineNode.Empty(SourceSpan.Unknown);

        _mockParser
            .Setup(static parser => parser.Parse(
                It.IsAny<ParsingContext>(),
                It.IsAny<DiagnosticBag>()))
            .Callback(static (ParsingContext _, DiagnosticBag diagnostics) =>
                diagnostics.Report(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidYaml,
                        SourceSpan.Unknown,
                        "test")))
            .Returns(parsedRoot);

        _mockExpander
            .Setup(static expander => expander.Expand(
                It.IsAny<PipelineNode>(),
                It.IsAny<ExpansionContext>(),
                It.IsAny<DiagnosticBag>()))
            .Returns(parsedRoot);

        var compiler = new PipelineAstCompiler(
            _mockParser.Object,
            _mockExpander.Object);

        var context = new CompilationContext();

        var source = new SourceContent(
            "not: valid: yaml:",
            TestSourceDocument.Default);

        compiler.Compile(
            source,
            context);

        Assert.True(
            context.Diagnostics.HasErrors);

        _mockExpander.Verify(
            expander => expander.Expand(
                parsedRoot,
                It.IsAny<ExpansionContext>(),
                context.Diagnostics),
            Times.Once);
    }
}
