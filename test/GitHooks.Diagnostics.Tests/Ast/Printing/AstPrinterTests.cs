using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Testing.Ast.Builders.Expressions;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;
using GitHooks.Testing.Common;

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
                .WithValue("Debug")
                .WithValue("Release"))
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
            .WithUnknownComplexKeyField(x => x
                .WithKey("key")
                .WithSequenceValue(s => s
                    .WithItem("item1")
                    .WithMappingItem(i => i
                        .WithField("nested", "value"))))
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
    public async Task Print_InvalidStep()
    {
        var step = new InvalidStepNodeBuilder()
            .WithField("displayName", "Test")
            .WithUnknownField("custom", "value")
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
        var span = TestSourceSpan.Create("pipeline.yaml", 1, 1, 1, 10);

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
            .WithSpan(TestSourceSpan.Unknown)
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
        var span = TestSourceSpan.Create("pipeline.yaml", 1, -1, -1, -1);

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
        var expression = new BooleanLiteralExpressionNodeBuilder()
            .WithValue(true)
            .Build();

        await VerifyAstAsync(expression);
    }

    [Fact]
    public async Task Print_InterpolatedString()
    {
        var expression = new InterpolatedStringExpressionNodeBuilder()
            .WithPart("/src/")
            .WithVariablePart("parameters.project")
            .Build();

        await VerifyAstAsync(expression);
    }
}
