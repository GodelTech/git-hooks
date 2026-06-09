using System.Runtime.CompilerServices;

using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepParserTests
{
    private readonly StepParser _parser = TestParserFactory.CreateStepParser();

    [Fact]
    public void Constructor_NullExpressionParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(null!, TestParserFactory.CreateUnknownNodeParser()));

        Assert.Equal(
            "expressionParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullUnknownNodeParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepParser(TestParserFactory.CreateExpressionParser(), null!));

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
    public void Parse_DuplicateField_Throws(
        string field,
        string firstValue,
        string secondValue)
    {
        var cursor =
            TestParserFactory.CreateCursor(
                $$"""
                {{field}}: {{firstValue}}
                {{field}}: {{secondValue}}
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            $"Duplicate '{field}' field",
            exception.Message);
    }

    [Theory]
    [InlineData(StepFieldNames.Env)]
    [InlineData(StepFieldNames.Parameters)]
    public void Parse_DuplicateDictionaryField_Throws(string field)
    {
        var cursor =
            TestParserFactory.CreateCursor(
                $$"""
                {{field}}:
                  name: configuration

                {{field}}:
                  name: framework
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            $"Duplicate '{field}' field",
            exception.Message);
    }

    [Fact]
    public void Parse_DuplicateEnvKey_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                script: dotnet test

                env:
                  CONFIGURATION: Debug
                  CONFIGURATION: Release
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Duplicate 'CONFIGURATION' field",
            exception.Message);
    }

    [Fact]
    public void Parse_DuplicateParameterKey_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                template: build.yml

                parameters:
                  configuration: Debug
                  configuration: Release
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Duplicate 'configuration' field",
            exception.Message);
    }

    [Fact]
    public void Parse_ScriptAndTemplate_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                script: dotnet test
                template: build.yml
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Step cannot contain multiple step type fields",
            exception.Message);
    }

    [Fact]
    public void Parse_ScriptStepWithParameters_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                script: dotnet test
                parameters:
                  configuration: Release
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Script step contains invalid field(s): parameters",
            exception.Message);
    }

    [Fact]
    public void Parse_TemplateStepWithDisplayName_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                template: build.yml
                displayName: Build
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Template step contains invalid field(s): displayName",
            exception.Message);
    }

    [Fact]
    public void Parse_MissingStepType_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                displayName: Test
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Step must contain exactly one step type field",
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
