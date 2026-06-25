using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Common;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Parameters;

public sealed class ParameterParserTests
{
    private readonly ParameterParser _parser
        = TestParserFactory.CreateParameterParser();

    [Fact]
    public void Constructor_NullFieldParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ParameterParser(
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
            () => new ParameterParser(
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
    [InlineData(ParameterFieldNames.Name, "test", "duplicate")]
    [InlineData(ParameterFieldNames.DisplayName, "Build", "Test")]
    [InlineData(ParameterFieldNames.Type, "number", "string")]
    [InlineData(ParameterFieldNames.DefaultValue, "abc", "def")]
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
                {{GetRequiredNamePrefix(field)}}
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
    public void Parse_DuplicateValuesField_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var firstSpan = TestSourceSpan.Create(document, 2, 1, 2, 7);
        var secondSpan = TestSourceSpan.Create(document, 5, 1, 5, 7);

        var context =
            TestParsingContextFactory.Create(
                """
                name: configuration
                values:
                  - Debug

                values:
                  - Release
                """,
                document);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "values");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            "First declaration is here.",
            firstSpan);

        Assert.NotNull(result.Values);

        var value = Assert.Single(result.Values.Items);

        Assert.Equal(
            "Debug",
            GetStringLiteralValue(value));
    }

    [Fact]
    public void Parse_MissingName_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 1, 1, 2, 1);

        var context =
            TestParsingContextFactory.Create(
                """
                type: string
                """,
                document);

        context.Cursor.StartDocument();

        var parameter = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.ParameterNameIsRequired,
            expectedSpan);

        AstAssert.HasStringField(
            parameter.Name,
            "name",
            string.Empty);

        AstAssert.HasStringField(
            parameter.Type!,
            "type",
            "string");
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

    private static string GetRequiredNamePrefix(
        string field)
    {
        return field is ParameterFieldNames.Name
            ? string.Empty
            : $"{ParameterFieldNames.Name}: configuration";
    }

    private static string GetStringLiteralValue(
        ExpressionNode? expression)
    {
        var literal = Assert.IsType<StringLiteralExpressionNode>(expression);

        return literal.Value;
    }

    private static void AssertFirstFieldValue(
        ParameterNode parameter,
        string field,
        string expected)
    {
        switch (field)
        {
            case ParameterFieldNames.Name:
                AstAssert.HasStringField(
                    parameter.Name,
                    ParameterFieldNames.Name,
                    expected);
                break;

            case ParameterFieldNames.DisplayName:
                AstAssert.HasStringField(
                    parameter.DisplayName!,
                    ParameterFieldNames.DisplayName,
                    expected);
                break;

            case ParameterFieldNames.Type:
                AstAssert.HasStringField(
                    parameter.Type!,
                    ParameterFieldNames.Type,
                    expected);
                break;

            case ParameterFieldNames.DefaultValue:
                AstAssert.HasStringField(
                    parameter.DefaultValue!,
                    ParameterFieldNames.DefaultValue,
                    expected);
                break;

            default:
                Assert.Fail($"Unexpected field '{field}'.");
                break;
        }
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
