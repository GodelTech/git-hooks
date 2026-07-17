using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;
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
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                script: dotnet test

                env:
                  [CONFIGURATION]: Release
                  FRAMEWORK: net10.0
                """,
                document);

        var complexKeySpan = TestSourceSpan.Create(document, 4, 3, 4, 27);
        var stepSpan = TestSourceSpan.Create(document, 1, 1, 6, 1);
        var envFrameworkFieldSpan = TestSourceSpan.Create(document, 5, 3, 5, 21);
        var envFrameworkFieldValueSpan = TestSourceSpan.Create(document, 5, 14, 5, 21);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MappingKeyMustBeScalar,
            complexKeySpan,
            "env");

        var scriptStep = StepAssert.IsScriptStep(
            result,
            stepSpan);

        Assert.NotNull(scriptStep.Env);

        var envField = Assert.Single(scriptStep.Env.Fields);

        FieldAssert.IsStringKeyField(
            envField,
            "FRAMEWORK",
            envFrameworkFieldSpan,
            "net10.0",
            envFrameworkFieldValueSpan);
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
    [InlineData(StepFieldNames.Script, typeof(string), "dotnet build", "dotnet test")]
    [InlineData(StepFieldNames.Template, typeof(string), "build.yml", "test.yml")]
    [InlineData(StepFieldNames.DisplayName, typeof(string), "Build", "Test")]
    [InlineData(StepFieldNames.Condition, typeof(string), "succeeded()", "failed()")]
    [InlineData(StepFieldNames.TimeoutInMinutes, typeof(int), "10", "20")]
    [InlineData(StepFieldNames.WorkingDirectory, typeof(string), "/src/tests", "/src/other")]
    public void Parse_DuplicateField_ReportsDiagnostic(
        string fieldName,
        Type fieldValueType,
        string firstValue,
        string secondValue)
    {
        ArgumentNullException.ThrowIfNull(fieldName);

        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                $$"""
                {{CreateRequiredStepTypeField(fieldName)}}

                {{fieldName}}: {{firstValue}}
                {{fieldName}}: {{secondValue}}
                """,
                document);

        var firstKeySpan = TestSourceSpan.CreateFieldKeySpan(document, 3, fieldName);
        var secondKeySpan = TestSourceSpan.CreateFieldKeySpan(document, 4, fieldName);
        var fieldSpan = TestSourceSpan.CreateFieldSpan(document, 3, fieldName, firstValue);
        var fieldValueSpan = TestSourceSpan.CreateFieldValueSpan(document, 3, fieldName, firstValue);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondKeySpan,
            fieldName);

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstKeySpan);

        FieldAssert.IsStringKeyField(
            StepAssert.GetRequiredField(result, fieldName),
            fieldName,
            fieldSpan,
            fieldValueType,
            firstValue,
            fieldValueSpan);
    }

    [Fact]
    public void Parse_DuplicateEnvKey_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                script: dotnet test

                env:
                  CONFIGURATION: Debug
                  CONFIGURATION: Release
                """,
                document);

        var firstKeySpan = TestSourceSpan.Create(document, 4, 3, 4, 16);
        var secondKeySpan = TestSourceSpan.Create(document, 5, 3, 5, 16);
        var stepSpan = TestSourceSpan.Create(document, 1, 1, 6, 1);
        var envConfigurationFieldSpan = TestSourceSpan.Create(document, 4, 3, 4, 23);
        var envConfigurationFieldValueSpan = TestSourceSpan.Create(document, 4, 18, 4, 23);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondKeySpan,
            "CONFIGURATION");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstKeySpan);

        var scriptStep = StepAssert.IsScriptStep(
            result,
            stepSpan);

        Assert.NotNull(scriptStep.Env);

        var envField = Assert.Single(scriptStep.Env.Fields);

        FieldAssert.IsStringKeyField(
            envField,
            "CONFIGURATION",
            envConfigurationFieldSpan,
            "Debug",
            envConfigurationFieldValueSpan);
    }

    [Fact]
    public void Parse_DuplicateParameterKey_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                template: build.yml

                parameters:
                  configuration: Debug
                  configuration: Release
                """,
                document);

        var firstKeySpan = TestSourceSpan.Create(document, 4, 3, 4, 16);
        var secondKeySpan = TestSourceSpan.Create(document, 5, 3, 5, 16);
        var stepSpan = TestSourceSpan.Create(document, 1, 1, 6, 1);
        var parametersConfigurationFieldSpan = TestSourceSpan.Create(document, 4, 3, 4, 23);
        var parametersConfigurationFieldValueSpan = TestSourceSpan.Create(document, 4, 18, 4, 23);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondKeySpan,
            "configuration");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstKeySpan);

        var templateStep = StepAssert.IsTemplateStep(
            result,
            stepSpan);

        Assert.NotNull(templateStep.Parameters);

        var parametersField = Assert.Single(templateStep.Parameters.Fields);

        FieldAssert.IsStringKeyField(
            parametersField,
            "configuration",
            parametersConfigurationFieldSpan,
            "Debug",
            parametersConfigurationFieldValueSpan);
    }

    [Fact]
    public async Task Parse_ScriptAndTemplate_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                script: dotnet test
                template: build.yml
                """,
                document);

        var stepSpan = TestSourceSpan.Create(document, 1, 1, 3, 1);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MultipleStepTypes,
            stepSpan);

        var invalidStep = StepAssert.IsInvalidStep(
            result,
            stepSpan);

        await VerifyAstAsync(invalidStep);
    }

    [Fact]
    public async Task Parse_ScriptStepWithParameters_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                script: dotnet test
                parameters:
                  configuration: Release
                """,
                document);

        var parametersSpan = TestSourceSpan.Create(document, 2, 1, 4, 1);
        var stepSpan = TestSourceSpan.Create(document, 1, 1, 4, 1);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            parametersSpan,
            "parameters",
            "script");

        var scriptStep = StepAssert.IsScriptStep(
            result,
            stepSpan);

        await VerifyAstAsync(scriptStep);
    }

    [Fact]
    public async Task Parse_TemplateStepWithDisplayName_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                template: build.yml
                displayName: Build
                """,
                document);

        var displayNameSpan = TestSourceSpan.Create(document, 2, 1, 2, 19);
        var stepSpan = TestSourceSpan.Create(document, 1, 1, 3, 1);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            displayNameSpan,
            "displayName",
            "template");

        var templateStep = StepAssert.IsTemplateStep(
            result,
            stepSpan);

        await VerifyAstAsync(templateStep);
    }

    [Fact]
    public async Task Parse_MissingStepType_ReturnsInvalidStep()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                displayName: Test
                """,
                document);

        var stepSpan = TestSourceSpan.Create(document, 1, 1, 2, 1);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MissingStepType,
            stepSpan);

        var invalidStep = StepAssert.IsInvalidStep(
            result,
            stepSpan);

        await VerifyAstAsync(invalidStep);
    }

    private static string CreateRequiredStepTypeField(string fieldName)
    {
        return fieldName is StepFieldNames.Script or StepFieldNames.Template
            ? string.Empty
            : $"{StepFieldNames.Script}: dotnet test";
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
