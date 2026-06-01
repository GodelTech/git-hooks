using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlPipelineParserTests
{
    private readonly YamlPipelineParser _parser = TestParserFactory.CreateYamlPipelineParser();

    [Fact]
    public void Constructor_NullPipelineParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new YamlPipelineParser(
                    null!));

        Assert.Equal(
            "pipelineParser",
            exception.ParamName);
    }

    [Fact]
    public void Parse_EmptyPipeline_ReturnsMissingStepsDiagnostic()
    {
        var result =
            _parser.Parse(
                "{}",
                "test.yaml");

        var diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            DiagnosticCode.InvalidYaml,
            diagnostic.Code);
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
        var result =
            _parser.Parse(
                """
                steps:
                  - script: test
                    invalid: [
                """,
                "test.yaml");

        Assert.Null(result.Root);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(
            DiagnosticCode.InvalidYaml,
            diagnostic.Code);

        Assert.Equal(
            DiagnosticSeverity.Error,
            diagnostic.Severity);
    }

    private async Task VerifyAstAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await TestParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            _parser.Parse,
            memberName,
            sourceFilePath);
    }
}
