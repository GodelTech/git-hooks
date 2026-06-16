using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Ast.Builders;

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
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("configuration")
                .WithDisplayName("Build Configuration")
                .WithValues(
                    "Debug",
                    "Release"))
            .WithScriptStep(x => x
                .WithScript("dotnet test")
                .WithDisplayName("Run Tests")
                .WithCondition("succeeded()")
                .WithTimeoutInMinutes(30)
                .WithWorkingDirectory("/src")
                .WithEnvironmentVariable("CONFIGURATION", "Release"))
            .WithTemplateStep(x => x
                .WithTemplate("build.yml")
                .WithParameter("configuration", "Release"))
            .WithUnknownField("simple", "value")
            .WithUnknownField(
                TestFields.ComplexKey(
                    key: "key",
                    value: TestValues.Sequence(
                        [
                            TestValues.Scalar("item1"),
                            TestValues.Mapping(
                                [
                                    TestFields.StringKey(
                                        key: "nested",
                                        value: "value")
                                ])
                        ])))
            .Build();

        await VerifyAstAsync(pipeline);
    }

    [Fact]
    public async Task Print_StringContainsEscapedCharacters()
    {
        var step = new ScriptStepNodeBuilder()
            .WithScript("echo \"Hello\"\nexit 0")
            .Build();

        await VerifyAstAsync(step);
    }

    [Fact]
    public async Task Print_IncludeNodeKinds()
    {
        var pipeline = new PipelineNodeBuilder()
            .Build();

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
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(1, 1),
            new SourcePosition(1, 10));

        var pipeline = new PipelineNodeBuilder()
            .WithSpan(span)
            .Build();

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
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("configuration")
                .WithDefaultValue("Release"))
            .Build();

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
        var pipeline = new PipelineNodeBuilder()
            .Build();

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
            new SourceDocument("pipeline.yaml"),
            new SourcePosition(1, -1),
            new SourcePosition(-1, -1));

        var pipeline = new PipelineNodeBuilder()
            .WithSpan(span)
            .Build();

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
        var expression = TestExpressions.Boolean(true);

        await VerifyAstAsync(expression);
    }

    [Fact]
    public async Task Print_InterpolatedString()
    {
        var expression =
            TestExpressions.InterpolatedString(
                TestExpressions.String("/src/"),
                TestExpressions.Variable("parameters.project"));

        await VerifyAstAsync(expression);
    }
}
