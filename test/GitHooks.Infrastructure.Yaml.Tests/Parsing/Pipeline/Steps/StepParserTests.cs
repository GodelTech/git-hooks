using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Diagnostics;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepParserTests
{
    private readonly StepParser _parser = TestParserFactory.CreateStepParser();

    [Fact]
    public void Constructor_NullExpressionParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(
                null!,
                TestParserFactory.CreateExpressionMappingParser(),
                TestParserFactory.CreateUnknownNodeParser()));

        Assert.Equal(
            "expressionParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullExpressionMappingParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(
                TestParserFactory.CreateExpressionParser(),
                null!,
                TestParserFactory.CreateUnknownNodeParser()));

        Assert.Equal(
            "expressionMappingParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullUnknownNodeParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(
                TestParserFactory.CreateExpressionParser(),
                TestParserFactory.CreateExpressionMappingParser(),
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
            TestParserFactory.CreateContext(
                """
                script: dotnet test

                env:
                  [CONFIGURATION]: Release
                  FRAMEWORK: net10.0
                """);

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
        var context =
            TestParserFactory.CreateContext(
                $$"""
                {{GetRequiredStepTypePrefix(field)}}
                {{field}}: {{firstValue}}
                {{field}}: {{secondValue}}
                """);

        context.Cursor.StartDocument();

        _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            field);

        // todo: solve duplicate field value assertion
        // AssertFirstFieldValue(
        //    result,
        //    field,
        //    firstValue);
    }

    [Fact]
    public void Parse_DuplicateEnvKey_ReportsDiagnostic()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                script: dotnet test

                env:
                  CONFIGURATION: Debug
                  CONFIGURATION: Release
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            "CONFIGURATION");

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
        var context =
            TestParserFactory.CreateContext(
                """
                template: build.yml

                parameters:
                  configuration: Debug
                  configuration: Release
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            "configuration");

        var templateStep = Assert.IsType<TemplateStepNode>(result);

        Assert.NotNull(templateStep.Parameters);

        var field = Assert.Single(templateStep.Parameters.Fields);

        AstAssert.HasStringField(
            field,
            "configuration",
            "Debug");
    }

    [Fact]
    public void Parse_ScriptAndTemplate_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                script: dotnet test
                template: build.yml
                """);

        context.Cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(context));

        Assert.StartsWith(
            "Step cannot contain multiple step type fields",
            exception.Message);
    }

    [Fact]
    public void Parse_ScriptStepWithParameters_ReportsDiagnostic()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                script: dotnet test
                parameters:
                  configuration: Release
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var step = Assert.IsType<ScriptStepNode>(result);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
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
        var context =
            TestParserFactory.CreateContext(
                """
                template: build.yml
                displayName: Build
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var step = Assert.IsType<TemplateStepNode>(result);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            "displayName",
            "template");

        AstAssert.HasStringField(
            step.Template,
            "template",
            "build.yml");
    }

    [Fact]
    public void Parse_MissingStepType_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                displayName: Test
                """);

        context.Cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(context));

        Assert.StartsWith(
            "Step must contain exactly one step type field",
            exception.Message);
    }

    private static string GetRequiredStepTypePrefix(string field)
    {
        return field is StepFieldNames.Script or StepFieldNames.Template
            ? string.Empty
            : $"{StepFieldNames.Script}: dotnet test";
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
