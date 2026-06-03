using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Ast.Visitors;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast;

using static GitHooks.Diagnostics.Tests.Ast.Printing.AstPrinterTestHelper;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

public sealed class AstPrinterTests
{
    [Fact]
    public void Print_NullNodeProvided_Throws()
    {
        var printer = new AstPrinter();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => printer.Print(null!));

        Assert.Equal(
            "node",
            exception.ParamName);
    }

    [Fact]
    public async Task Print_FullFeaturedPipeline()
    {
        var pipeline =
            TestAst.Pipeline(
                parameters:
                [
                    TestAst.Parameter(
                        name: "configuration",
                        displayName: "Build Configuration",
                        values:
                        [
                            "Debug",
                            "Release"
                        ])
                ],
                steps:
                [
                    TestAst.ScriptStep(
                        script: "dotnet test",
                        displayName: "Run Tests",
                        condition: "succeeded()",
                        timeoutInMinutes: 30,
                        workingDirectory: "/src",
                        env: new Dictionary<string, string>
                        {
                            ["CONFIGURATION"] = "Release"
                        }),

                    TestAst.TemplateStep(
                        template: "build.yml",
                        parameters: new Dictionary<string, string>
                        {
                            ["configuration"] = "Release"
                        })
                ],
                unknownFields:
                [
                    TestAst.UnknownSimpleField(
                        key: "simple",
                        value: "value"),

                    TestAst.UnknownComplexField(
                        key: "key",
                        value: TestAst.UnknownSequence(
                            items:
                            [
                                TestAst.UnknownScalar("item1"),
                                TestAst.UnknownMapping(
                                    fields:
                                    [
                                        TestAst.UnknownSimpleField(
                                            key: "nested",
                                            value: "value")
                                    ])
                            ]))
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
                    defaultValue: "Release")
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

    [Fact]
    public async Task Print_BooleanLiteral()
    {
        var expression = TestAst.BooleanLiteral(true);

        await VerifyAstAsync(expression);
    }

    [Fact]
    public async Task Print_InterpolatedString()
    {
        var expression =
            TestAst.InterpolatedString(
                TestAst.StringLiteral("/src/"),
                TestAst.Variable("parameters.project"));

        await VerifyAstAsync(expression);
    }

    [Fact]
    public void VisitUnknownNode_UnsupportedNode_Throws()
    {
        var printer = new AstPrinter();

        var node = new FakeUnknownNode
        {
            Span = SourceSpan.Unknown
        };

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => printer.VisitUnknownNode(node));

        Assert.Equal(
            "Unsupported unknown node 'FakeUnknownNode'.",
            exception.Message);
    }

    private sealed record FakeUnknownNode
        : UnknownNode
    {
        public override AstNodeKind Kind
            => AstNodeKind.UnknownScalar;

        public override void Accept(IAstCommandVisitor visitor)
        {
            visitor.VisitUnknownNode(this);
        }

        public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
        {
            return visitor.VisitUnknownNode(this);
        }
    }
}
