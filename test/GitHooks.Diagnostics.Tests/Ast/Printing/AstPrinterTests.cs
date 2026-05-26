using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

public sealed class AstPrinterTests
{
    [Fact]
    public void Print_PipelineProvided_ReturnsFormattedTree()
    {
        var pipeline = TestAst.Pipeline(
            parameters:
            [
                TestAst.Parameter(
                    name: "configuration",
                    defaultValue: "Release")
            ],
            steps:
            [
                TestAst.Script(
                    script: "dotnet test")
            ]);

        var printer = new AstPrinter();

        var result = printer.Print(pipeline);

        Assert.Equal(
            """
            Pipeline
              Parameter(configuration)
                Type(String)
                DefaultValue("Release")
              ScriptStep
                String("dotnet test")
            """,
            result);
    }

    [Fact]
    public void Print_StringContainsEscapedCharacters_ReturnsEscapedString()
    {
        var step = TestAst.Script(
            script: "echo \"Hello\"\nexit 0");

        var printer = new AstPrinter();

        var result = printer.Print(step);

        Assert.Equal(
            """
            ScriptStep
              String("echo \"Hello\"\nexit 0")
            """,
            result);
    }

    [Fact]
    public void Print_IncludeNodeKindsEnabled_ReturnsNodeKinds()
    {
        var pipeline = TestAst.Pipeline();

        var printer = new AstPrinter(
            new AstPrinterOptions
            {
                IncludeNodeKinds = true
            });

        var result = printer.Print(pipeline);

        Assert.Equal(
            "Pipeline [Pipeline]",
            result);
    }

    [Fact]
    public void Print_IncludeSourceSpansEnabled_ReturnsRenderedSpans()
    {
        var span = new SourceSpan(
            new SourcePosition(1, 1),
            new SourcePosition(1, 10));

        var pipeline = TestAst.Pipeline(
            span: span);

        var printer = new AstPrinter(
            new AstPrinterOptions
            {
                IncludeSourceSpans = true
            });

        var result = printer.Print(pipeline);

        Assert.Equal(
            "Pipeline @ (1:1-1:10)",
            result);
    }

    [Fact]
    public void Print_CustomIndentSizeProvided_ReturnsIndentedOutput()
    {
        var pipeline = TestAst.Pipeline(
            parameters:
            [
                TestAst.Parameter(
                    name: "configuration",
                    defaultValue: "Release")
            ]);

        var printer = new AstPrinter(
            new AstPrinterOptions
            {
                IndentSize = 4
            });

        var result = printer.Print(pipeline);

        Assert.Equal(
            """
            Pipeline
                Parameter(configuration)
                    Type(String)
                    DefaultValue("Release")
            """,
            result);
    }

    [Fact]
    public void Print_UnknownSourceSpanProvided_ReturnsUnknownSpan()
    {
        var pipeline = TestAst.Pipeline();

        var printer = new AstPrinter(
            new AstPrinterOptions
            {
                IncludeSourceSpans = true
            });

        var result = printer.Print(pipeline);

        Assert.Equal(
            "Pipeline @ <unknown>",
            result);
    }

    [Fact]
    public void Print_PartiallyKnownSourceSpanProvided_ReturnsPartialSpan()
    {
        var span = new SourceSpan(
            new SourcePosition(1, -1),
            new SourcePosition(-1, -1));

        var pipeline = TestAst.Pipeline(
            span: span);

        var printer = new AstPrinter(
            new AstPrinterOptions
            {
                IncludeSourceSpans = true
            });

        var result = printer.Print(pipeline);

        Assert.Equal(
            "Pipeline @ (1:?-<unknown>)",
            result);
    }
}
