using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;

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
        var context =
            TestParserFactory.CreateContext(
                """
                parameters: []
                parameters: []
                steps:
                  - script: dotnet test
                """);

        context.Cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(context));

        Assert.StartsWith(
            "Duplicate 'parameters' field",
            exception.Message);
    }

    [Fact]
    public void Parse_DuplicateStepsField_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                steps:
                  - script: dotnet test

                steps:
                  - script: dotnet build
                """);

        context.Cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(context));

        Assert.StartsWith(
            "Duplicate 'steps' field",
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
