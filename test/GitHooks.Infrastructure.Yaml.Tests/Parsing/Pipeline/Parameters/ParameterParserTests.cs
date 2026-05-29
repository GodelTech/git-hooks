using System.Runtime.CompilerServices;

using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Parameters;

public sealed class ParameterParserTests
{
    private readonly ParameterParser _parser = TestParserFactory.CreateParameterParser();

    [Fact]
    public void Constructor_NullExpressionParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ParameterParser(null!, TestParserFactory.CreateUnknownNodeParser()));

        Assert.Equal(
            "expressionParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullUnknownNodeParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ParameterParser(TestParserFactory.CreateExpressionParser(), null!));

        Assert.Equal(
            "unknownNodeParser",
            exception.ParamName);
    }

    [Fact]
    public async Task Parse_NonScalarMappingKey()
    {
        await VerifyAstAsync(
            """
            name: configuration

            [1, 2]: value
            """);
    }

    [Fact]
    public async Task Parse_Parameter()
    {
        await VerifyAstAsync(
            """
            name: configuration
            """);
    }

    [Fact]
    public async Task Parse_ParameterWithAllFields()
    {
        await VerifyAstAsync(
            """
            name: configuration
            displayName: Configuration
            type: string
            default: Release
            values:
              - Debug
              - Release
            """);
    }

    [Fact]
    public async Task Parse_ParameterWithUnknownField()
    {
        await VerifyAstAsync(
            """
            name: configuration

            custom:
              nested:
                - value
            """);
    }

    [Theory]
    [InlineData("string", ParameterType.String)]
    [InlineData("boolean", ParameterType.Boolean)]
    [InlineData("number", ParameterType.Number)]
    [InlineData("object", ParameterType.Object)]
    public void Parse_ValidType_ReturnsParameterType(
        string value,
        ParameterType expected)
    {
        var cursor =
            TestParserFactory.CreateCursor(
                $$"""
                name: configuration
                type: {{value}}
                """);

        cursor.StartDocument();

        var result = _parser.Parse(cursor);

        cursor.EndDocument();

        Assert.Equal(
            expected,
            result.Type);
    }

    [Fact]
    public void Parse_UnsupportedType_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                name: configuration
                type: invalid
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Unsupported parameter type 'invalid'",
            exception.Message);
    }

    [Theory]
    [InlineData(ParameterFieldNames.Name, "test", "duplicate")]
    [InlineData(ParameterFieldNames.DisplayName, "Build", "Test")]
    [InlineData(ParameterFieldNames.Type, "string", "number")]
    [InlineData(ParameterFieldNames.DefaultValue, "abc", "def")]
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
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            $"Duplicate '{field}' field",
            exception.Message);
    }

    [Fact]
    public void Parse_DuplicateValuesField_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                values:
                  - Debug

                values:
                  - Release
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Duplicate 'values' field",
            exception.Message);
    }

    [Fact]
    public void Parse_MissingName_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                type: string
                """);

        cursor.StartDocument();

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _parser.Parse(cursor));

        Assert.StartsWith(
            "Parameter requires 'name'",
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
