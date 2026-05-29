using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlPipelineParserTests
{
    private readonly YamlPipelineParser _parser = TestParserFactory.CreateYamlPipelineParser();

    [Fact]
    public async Task Parse_EmptyPipeline()
    {
        await VerifyAstAsync(
            "{}");
    }

    [Fact]
    public async Task Parse_SimpleScriptStep()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo hello
            """);
    }

    [Fact]
    public async Task Parse_Parameter()
    {
        await VerifyAstAsync(
            """
            parameters:
              - name: configuration
                displayName: Build configuration
                type: String
                default: Debug
                values:
                  - Debug
                  - Release
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
    public async Task Parse_UnknownStepField()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo hello
                retry: 3
            """);
    }

    [Fact]
    public async Task Parse_UnknownNestedSequence()
    {
        await VerifyAstAsync(
            """
            unknown:
              - - - value
            """);
    }

    [Fact]
    public async Task Parse_UnknownNestedMapping()
    {
        await VerifyAstAsync(
            """
            unknown:
              nested:
                child:
                  value: test
            """);
    }

    [Fact]
    public async Task Parse_MultipleSteps()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo first

              - script: echo second
            """);
    }

    [Fact]
    public async Task Parse_Parameter_WithUnknownField()
    {
        await VerifyAstAsync(
            """
            parameters:
              - name: configuration
                custom: value
            """);
    }

    [Fact]
    public async Task Parse_Step_WithUnknownSequence()
    {
        await VerifyAstAsync(
            """
            steps:
              - script: echo hello
                matrix:
                  - linux
                  - windows
            """);
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
