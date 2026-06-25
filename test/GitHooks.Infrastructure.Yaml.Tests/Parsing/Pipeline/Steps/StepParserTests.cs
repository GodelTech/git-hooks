using System.Globalization;
using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepParserTests
{
    private readonly StepParser _parser
        = TestParserFactory.CreateStepParser();

    [Fact]
    public void Constructor_NullFieldParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(
                null!,
                TestParserFactory.CreateFieldValueParser()));

        Assert.Equal(
            "fieldParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullFieldValueParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(
                TestParserFactory.CreateFieldParser(),
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
            script: dotnet test

            [1, 2]: value
            """);
    }

    [Fact]
    public async Task Parse_ScriptStep()
    {
        await VerifyAstAsync(
            """
            script: dotnet test
            """);
    }

    [Fact]
    public async Task Parse_ScriptStepWithAllFields()
    {
        await VerifyAstAsync(
            """
            script: dotnet test
            displayName: Run tests
            condition: succeeded()
            timeoutInMinutes: 10
            workingDirectory: /src/tests
            env:
              CONFIGURATION: Release
              FRAMEWORK: net9.0
            """);
    }

    [Fact]
    public async Task Parse_ScriptStepWithInterpolation()
    {
        await VerifyAstAsync(
            """
            script: dotnet test
            workingDirectory: /src/${{ parameters.project }}
            """);
    }

    [Fact]
    public async Task Parse_ScriptStepWithUnknownField()
    {
        await VerifyAstAsync(
            """
            script: dotnet test

            custom:
              nested:
                - value
            """);
    }

    [Fact]
    public void Parse_ScriptStepWithNonScalarEnvKey_SkipsEntry()
    {
        var context =
            TestParsingContextFactory.Create(
                """
                script: dotnet test

                env:
                  [CONFIGURATION]: Release
                  FRAMEWORK: net10.0
                """,
                TestSourceDocument.Default);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var scriptStep = Assert.IsType<ScriptStepNode>(result);

        Assert.NotNull(scriptStep.Env);

        var field = Assert.Single(scriptStep.Env.Fields);

        AstAssert.HasStringField(
            field,
            "FRAMEWORK",
            "net10.0");
    }

    [Fact]
    public async Task Parse_TemplateStep()
    {
        await VerifyAstAsync(
            """
            template: build.yml
            """);
    }

    [Fact]
    public async Task Parse_TemplateStepWithParameters()
    {
        await VerifyAstAsync(
            """
            template: build.yml
            parameters:
              configuration: Release
              framework: net9.0
            """);
    }

    [Fact]
    public async Task Parse_TemplateStepWithExpressionParameter()
    {
        await VerifyAstAsync(
            """
            template: build.yml
            parameters:
              configuration: ${{ parameters.configuration }}
            """);
    }

    [Fact]
    public async Task Parse_TemplateStepWithUnknownField()
    {
        await VerifyAstAsync(
            """
            template: build.yml

            custom:
              nested:
                - value
            """);
    }

    [Theory]
    [InlineData(StepFieldNames.Script, "dotnet build", "dotnet test")]
    [InlineData(StepFieldNames.Template, "build.yml", "test.yml")]
    [InlineData(StepFieldNames.DisplayName, "Build", "Test")]
    [InlineData(StepFieldNames.Condition, "succeeded()", "failed()")]
    [InlineData(StepFieldNames.TimeoutInMinutes, "10", "20")]
    [InlineData(StepFieldNames.WorkingDirectory, "/src/tests", "/src/other")]
    public void Parse_DuplicateField_ReportsDiagnostic(
        string field,
        string firstValue,
        string secondValue)
    {
        ArgumentNullException.ThrowIfNull(field);

        var document = TestSourceDocument.Default;

        var firstSpan = CreateFieldSpan(document, 2, field);
        var secondSpan = CreateFieldSpan(document, 3, field);

        var context =
            TestParsingContextFactory.Create(
                $$"""
                {{GetRequiredStepTypePrefix(field)}}
                {{field}}: {{firstValue}}
                {{field}}: {{secondValue}}
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            field);

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            "First declaration is here.",
            firstSpan);

        AssertFirstFieldValue(
            result,
            field,
            firstValue);
    }

    [Fact]
    public void Parse_DuplicateEnvKey_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var firstSpan = TestSourceSpan.Create(document, 4, 3, 4, 16);
        var secondSpan = TestSourceSpan.Create(document, 5, 3, 5, 16);

        var context =
            TestParsingContextFactory.Create(
                """
                script: dotnet test

                env:
                  CONFIGURATION: Debug
                  CONFIGURATION: Release
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "CONFIGURATION");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            "First declaration is here.",
            firstSpan);

        var scriptStep = Assert.IsType<ScriptStepNode>(result);

        Assert.NotNull(scriptStep.Env);

        var field = Assert.Single(scriptStep.Env.Fields);

        AstAssert.HasStringField(
            field,
            "CONFIGURATION",
            "Debug");
    }

    [Fact]
    public void Parse_DuplicateParameterKey_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var firstSpan = TestSourceSpan.Create(document, 4, 3, 4, 16);
        var secondSpan = TestSourceSpan.Create(document, 5, 3, 5, 16);

        var context =
            TestParsingContextFactory.Create(
                """
                template: build.yml

                parameters:
                  configuration: Debug
                  configuration: Release
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "configuration");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            "First declaration is here.",
            firstSpan);

        var templateStep = Assert.IsType<TemplateStepNode>(result);

        Assert.NotNull(templateStep.Parameters);

        var field = Assert.Single(templateStep.Parameters.Fields);

        AstAssert.HasStringField(
            field,
            "configuration",
            "Debug");
    }

    [Fact]
    public async Task Parse_ScriptAndTemplate_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 1, 1, 3, 1);

        var context =
            TestParsingContextFactory.Create(
                """
                script: dotnet test
                template: build.yml
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MultipleStepTypes,
            expectedSpan);

        var invalidStep = Assert.IsType<InvalidStepNode>(result);

        await VerifyAstAsync(invalidStep);
    }

    [Fact]
    public void Parse_ScriptStepWithParameters_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 2, 1, 4, 1);

        var context =
            TestParsingContextFactory.Create(
                """
                script: dotnet test
                parameters:
                  configuration: Release
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var step = Assert.IsType<ScriptStepNode>(result);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            expectedSpan,
            "parameters",
            "script");

        AstAssert.HasStringField(
            step.Script,
            "script",
            "dotnet test");
    }

    [Fact]
    public void Parse_TemplateStepWithDisplayName_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 2, 1, 2, 19);

        var context =
            TestParsingContextFactory.Create(
                """
                template: build.yml
                displayName: Build
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var step = Assert.IsType<TemplateStepNode>(result);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            expectedSpan,
            "displayName",
            "template");

        AstAssert.HasStringField(
            step.Template,
            "template",
            "build.yml");
    }

    [Fact]
    public void Parse_MissingStepType_ReturnsInvalidStep()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 1, 1, 2, 1);

        var context =
            TestParsingContextFactory.Create(
                """
                displayName: Test
                """,
                document);

        context.Cursor.StartDocument();

        var step = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MissingStepType,
            expectedSpan);

        var invalidStep = Assert.IsType<InvalidStepNode>(step);

        Assert.Single(invalidStep.Fields);

        var field = Assert.IsType<StringKeyFieldNode<ExpressionNode>>(invalidStep.Fields[0]);

        AstAssert.HasStringField(
            field,
            "displayName",
            "Test");
    }

    private static SourceSpan CreateFieldSpan(
        SourceDocument document,
        int line,
        string field)
    {
        return TestSourceSpan.Create(
            document,
            line,
            1,
            line,
            field.Length + 1);
    }

    private static string GetRequiredStepTypePrefix(string field)
    {
        return field is StepFieldNames.Script or StepFieldNames.Template
            ? string.Empty
            : $"{StepFieldNames.Script}: dotnet test";
    }

    private static void AssertFirstFieldValue(
        StepNode step,
        string field,
        string expected)
    {
        switch (field)
        {
            case StepFieldNames.Script:
                AstAssert.HasStringField(
                    Assert.IsType<ScriptStepNode>(step).Script,
                    StepFieldNames.Script,
                    expected);
                break;

            case StepFieldNames.Template:
                AstAssert.HasStringField(
                    Assert.IsType<TemplateStepNode>(step).Template,
                    StepFieldNames.Template,
                    expected);
                break;

            case StepFieldNames.DisplayName:
                AstAssert.HasStringField(
                    Assert.IsType<ScriptStepNode>(step).DisplayName!,
                    StepFieldNames.DisplayName,
                    expected);
                break;

            case StepFieldNames.Condition:
                AstAssert.HasStringField(
                    Assert.IsType<ScriptStepNode>(step).Condition!,
                    StepFieldNames.Condition,
                    expected);
                break;

            case StepFieldNames.TimeoutInMinutes:
                AstAssert.HasIntegerField(
                    Assert.IsType<ScriptStepNode>(step).TimeoutInMinutes!,
                    StepFieldNames.TimeoutInMinutes,
                    int.Parse(expected, CultureInfo.InvariantCulture));
                break;

            case StepFieldNames.WorkingDirectory:
                AstAssert.HasStringField(
                    Assert.IsType<ScriptStepNode>(step).WorkingDirectory!,
                    StepFieldNames.WorkingDirectory,
                    expected);
                break;

            default:
                Assert.Fail($"Unexpected field '{field}'.");
                break;
        }
    }

    private static async Task VerifyAstAsync(
        AstNode node,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await ParserSnapshotVerifier.VerifyAstAsync(
            node,
            memberName,
            sourceFilePath);
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
