using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast;

using static GitHooks.Diagnostics.Tests.Ast.Printing.AstPrinterTestHelper;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

public sealed class AstPrinterTests
{
    [Fact]
    public async Task Print_Pipeline()
    {
        var pipeline = TestAst.Pipeline(
            parameters:
            [
                TestAst.Parameter(
                    name: "configuration",
                    defaultValue: TestAst.StringLiteral(
                        "Release"))
            ],
            steps:
            [
                TestAst.ScriptStep(
                    script: "dotnet test")
            ]);

        await VerifyAstAsync(pipeline);
    }

    [Fact]
    public async Task Print_StringContainsEscapedCharacters()
    {
        var step = TestAst.ScriptStep(
            script: "echo \"Hello\"\nexit 0");

        await VerifyAstAsync(step);
    }

    [Fact]
    public async Task Print_IncludeNodeKinds()
    {
        var pipeline = TestAst.Pipeline();

        await VerifyAstAsync(
            pipeline,
            new AstPrinterOptions
            {
                IncludeNodeKinds = true
            });
    }

    [Fact]
    public async Task Print_IncludeSourceSpans()
    {
        var span = new SourceSpan(
            new SourcePosition(1, 1),
            new SourcePosition(1, 10));

        var pipeline = TestAst.Pipeline(
            span: span);

        await VerifyAstAsync(
            pipeline,
            new AstPrinterOptions
            {
                IncludeSourceSpans = true
            });
    }

    [Fact]
    public async Task Print_CustomIndentSize()
    {
        var pipeline = TestAst.Pipeline(
            parameters:
            [
                TestAst.Parameter(
                    name: "configuration",
                    defaultValue: TestAst.StringLiteral(
                        "Release"))
            ]);

        await VerifyAstAsync(
            pipeline,
            new AstPrinterOptions
            {
                IndentSize = 4
            });
    }

    [Fact]
    public async Task Print_UnknownSourceSpan()
    {
        var pipeline = TestAst.Pipeline();

        await VerifyAstAsync(
            pipeline,
            new AstPrinterOptions
            {
                IncludeSourceSpans = true
            });
    }

    [Fact]
    public async Task Print_PartiallyKnownSourceSpan()
    {
        var span = new SourceSpan(
            new SourcePosition(1, -1),
            new SourcePosition(-1, -1));

        var pipeline = TestAst.Pipeline(
            span: span);

        await VerifyAstAsync(
            pipeline,
            new AstPrinterOptions
            {
                IncludeSourceSpans = true
            });
    }
}
