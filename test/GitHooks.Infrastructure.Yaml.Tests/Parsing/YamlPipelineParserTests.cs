using System.Runtime.CompilerServices;

using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlPipelineParserTests
{
    private readonly YamlPipelineParser _parser
        = TestParserFactory.CreateYamlPipelineParser();

    [Fact]
    public void Constructor_NullPipelineParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new YamlPipelineParser(
                null!));

        Assert.Equal(
            "pipelineParser",
            exception.ParamName);
    }

    [Fact]
    public async Task Parse_MinimalPipeline()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo hello
            """);
    }

    [Fact]
    public async Task Parse_Pipeline()
    {
        await VerifyAstAsync(
            """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: dotnet restore

              - script: dotnet build
                displayName: Build
            """);
    }

    [Fact]
    public async Task Parse_TemplateStep()
    {
        await VerifyAstAsync(
            """
            steps:
              - template: build.yml
            """);
    }

    [Fact]
    public async Task Parse_UnknownField()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo hello

            custom:
              nested:
                - value
            """);
    }

    [Fact]
    public async Task Parse_UnknownComplexField()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo hello

            ? [1, 2]
            : value
            """);
    }

    [Fact]
    public void Parse_InvalidYaml_ReturnsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 4, 1, 4, 1);

        var context = new ParsingContext(
            """
            steps:
              - script: test
                invalid: [
            """,
            document);

        var diagnostics = new DiagnosticBag();

        var result = _parser.Parse(context, diagnostics);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.InvalidYaml,
            expectedSpan,
            "While parsing a node, did not find expected node content.");

        PipelineAssert.IsEmptyPipeline(
            result,
            expectedSpan);
    }

    private async Task VerifyAstAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await ParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            _parser.Parse,
            memberName,
            sourceFilePath);
    }
}
