using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline;

public sealed class PipelineParserTests
{
    private readonly PipelineParser _parser = TestParserFactory.CreatePipelineParser();

    [Fact]
    public void Constructor_NullParametersParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new PipelineParser(
                    null!,
                    TestParserFactory.CreateStepsParser(),
                    TestParserFactory.CreateUnknownNodeParser()));

        Assert.Equal(
            "parametersParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullStepsParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new PipelineParser(
                    TestParserFactory.CreateParametersParser(),
                    null!,
                    TestParserFactory.CreateUnknownNodeParser()));

        Assert.Equal(
            "stepsParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullUnknownNodeParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new PipelineParser(
                    TestParserFactory.CreateParametersParser(),
                    TestParserFactory.CreateStepsParser(),
                    null!));

        Assert.Equal(
            "unknownNodeParser",
            exception.ParamName);
    }

    [Fact]
    public async Task Parse_NonScalarMappingKey()
    {
        await VerifyAstAsync(
            """
            parameters:
              - name: configuration
                type: string

            [1, 2]: value

            steps:
              - script: dotnet test
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

            steps:
              - script: dotnet test
            """);
    }

    [Fact]
    public async Task Parse_PipelineWithUnknownField()
    {
        await VerifyAstAsync(
            """
            parameters:
              - name: configuration
                type: string

            custom:
              nested:
                - value

            steps:
              - script: dotnet test
            """);
    }

    [Fact]
    public void Parse_DuplicateParametersField_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                parameters: []
                parameters: []
                steps:
                  - script: dotnet test
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Duplicate 'parameters' field",
            exception.Message);
    }

    [Fact]
    public void Parse_DuplicateStepsField_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                steps:
                  - script: dotnet test

                steps:
                  - script: dotnet build
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Duplicate 'steps' field",
            exception.Message);
    }

    [Fact]
    public void Parse_MissingSteps_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                parameters:
                  - name: configuration
                    type: string
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Pipeline must contain at least one step",
            exception.Message);
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
