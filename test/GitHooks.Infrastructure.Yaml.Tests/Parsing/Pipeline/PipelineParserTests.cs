using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline;

public sealed class PipelineParserTests
{
    private readonly PipelineParser _parser
        = TestParserFactory.CreatePipelineParser();

    [Fact]
    public void Constructor_NullParametersParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineParser(
                null!,
                TestParserFactory.CreateStepsParser(),
                TestParserFactory.CreateFieldValueParser()));

        Assert.Equal(
            "parametersParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullStepsParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineParser(
                TestParserFactory.CreateParametersParser(),
                null!,
                TestParserFactory.CreateFieldValueParser()));

        Assert.Equal(
            "stepsParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullFieldValueParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PipelineParser(
                TestParserFactory.CreateParametersParser(),
                TestParserFactory.CreateStepsParser(),
                null!));

        Assert.Equal(
            "fieldValueParser",
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
    public void Parse_DuplicateParametersField_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                parameters: []
                parameters: []
                steps:
                  - script: dotnet test
                """,
                document);

        var firstSpan = TestSourceSpan.Create(document, 1, 1, 1, 11);
        var secondSpan = TestSourceSpan.Create(document, 2, 1, 2, 11);
        var pipelineSpan = TestSourceSpan.Create(document, 1, 1, 5, 1);
        var stepSpan = TestSourceSpan.Create(document, 4, 5, 5, 1);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "parameters");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstSpan);

        PipelineAssert.IsPipeline(
            result,
            0,
            1,
            pipelineSpan);

        PipelineAssert.SingleScriptStep(
            result,
            stepSpan);
    }

    [Fact]
    public void Parse_DuplicateStepsField_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                steps:
                  - script: dotnet test

                steps:
                  - script: dotnet build
                """,
                document);

        var firstSpan = TestSourceSpan.Create(document, 1, 1, 1, 6);
        var secondSpan = TestSourceSpan.Create(document, 4, 1, 4, 6);
        var pipelineSpan = TestSourceSpan.Create(document, 1, 1, 6, 1);
        var stepSpan = TestSourceSpan.Create(document, 2, 5, 4, 1);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "steps");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstSpan);

        PipelineAssert.IsPipeline(
            result,
            0,
            1,
            pipelineSpan);

        PipelineAssert.SingleScriptStep(
            result,
            stepSpan);
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
