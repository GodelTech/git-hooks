using static GitHooks.Infrastructure.Yaml.Tests.Parsing.YamlParserTestHelper;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlPipelineParserSnapshotTests
{
    [Fact]
    public async Task Parse_EmptyPipeline()
    {
        await VerifyAst(
            "{}");
    }

    [Fact]
    public async Task Parse_SimpleScriptStep()
    {
        await VerifyAst(
            """
            steps:
              - script: echo hello
            """);
    }

    [Fact]
    public async Task Parse_Parameter()
    {
        await VerifyAst(
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
        await VerifyAst(
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
        await VerifyAst(
            """
            steps:
              - script: echo hello
                retry: 3
            """);
    }

    [Fact]
    public async Task Parse_UnknownNestedSequence()
    {
        await VerifyAst(
            """
            unknown:
              - - - value
            """);
    }

    [Fact]
    public async Task Parse_UnknownNestedMapping()
    {
        await VerifyAst(
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
        await VerifyAst(
            """
            steps:
              - script: echo first

              - script: echo second
            """);
    }

    [Fact]
    public async Task Parse_Parameter_WithUnknownField()
    {
        await VerifyAst(
            """
            parameters:
              - name: configuration
                custom: value
            """);
    }

    [Fact]
    public async Task Parse_Step_WithUnknownSequence()
    {
        await VerifyAst(
            """
            steps:
              - script: echo hello
                matrix:
                  - linux
                  - windows
            """);
    }
}
