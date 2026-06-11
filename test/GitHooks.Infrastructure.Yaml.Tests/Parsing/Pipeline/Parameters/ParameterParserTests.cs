using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Diagnostics;

using YamlDotNet.Core;

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
        var context =
            TestParserFactory.CreateContext(
                $$"""
                name: configuration
                type: {{value}}
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        Assert.Equal(
            expected,
            result.Type);
    }

    [Fact]
    public void Parse_UnsupportedType_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                name: configuration
                type: invalid
                """);

        context.Cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(context));

        Assert.StartsWith(
            "Unsupported parameter type 'invalid'",
            exception.Message);
    }

    [Theory]
    [InlineData(ParameterFieldNames.Name, "test", "duplicate")]
    [InlineData(ParameterFieldNames.DisplayName, "Build", "Test")]
    [InlineData(ParameterFieldNames.Type, "string", "number")]
    [InlineData(ParameterFieldNames.DefaultValue, "abc", "def")]
    public void Parse_DuplicateField_ReportsDiagnostic(
        string field,
        string firstValue,
        string secondValue)
    {
        var context =
            TestParserFactory.CreateContext(
                $$"""
                {{GetRequiredNamePrefix(field)}}
                {{field}}: {{firstValue}}
                {{field}}: {{secondValue}}
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField);

        Assert.Equal(
            $"Duplicate '{field}' field.",
            diagnostic.Message);

        switch (field)
        {
            case ParameterFieldNames.Name:
                Assert.Equal(
                    firstValue,
                    result.Name);
                break;

            case ParameterFieldNames.DisplayName:
                Assert.Equal(
                    firstValue,
                    result.DisplayName);
                break;

            case ParameterFieldNames.Type:
                Assert.Equal(
                    ParameterType.String,
                    result.Type);
                break;

            case ParameterFieldNames.DefaultValue:
                Assert.Equal(
                    firstValue,
                    GetStringLiteralValue(result.DefaultValue));
                break;

            default:
                break;
        }
    }

    [Fact]
    public void Parse_DuplicateValuesField_ReportsDiagnostic()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                name: configuration
                values:
                  - Debug

                values:
                  - Release
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField);

        Assert.Equal(
            "Duplicate 'values' field.",
            diagnostic.Message);

        var value = Assert.Single(result.Values);

        Assert.Equal(
            "Debug",
            GetStringLiteralValue(value));
    }

    [Fact]
    public void Parse_MissingName_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                type: string
                """);

        context.Cursor.StartDocument();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.Parse(context));

        Assert.StartsWith(
            "Parameter requires 'name'",
            exception.Message);
    }

    private static string GetRequiredNamePrefix(string field)
    {
        return field == ParameterFieldNames.Name
            ? string.Empty
            : $"{ParameterFieldNames.Name}: configuration";
    }

    private static string GetStringLiteralValue(ExpressionNode? expression)
    {
        var literal = Assert.IsType<StringLiteralExpressionNode>(expression);

        return literal.Value;
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
