using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
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
        string fieldName,
        string firstValue,
        string secondValue)
    {
        ArgumentNullException.ThrowIfNull(fieldName);

        var document = TestSourceDocument.Default;

        var context =
            TestParsingContextFactory.Create(
                $$"""
                {{CreateRequiredNameField(fieldName)}}

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
            "First declaration is here.",
            firstKeySpan);

        FieldAssert.IsStringKeyField(
            ParameterAssert.GetRequiredField(result, fieldName),
            fieldName,
            fieldSpan,
            firstValue,
            fieldValueSpan);
    }

    [Fact]
    public void Parse_DuplicateValuesField_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

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

        var firstKeySpan = TestSourceSpan.Create(document, 2, 1, 2, 7);
        var secondKeySpan = TestSourceSpan.Create(document, 5, 1, 5, 7);
        var valuesFieldSpan = TestSourceSpan.Create(document, 2, 1, 5, 1);
        var valuesFieldValueSpan = TestSourceSpan.Create(document, 3, 5, 3, 10);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondKeySpan,
            "values");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            "First declaration is here.",
            firstKeySpan);

        FieldAssert.IsSequenceFieldWithItems(
            result.Values!,
            "values",
            valuesFieldSpan);

        FieldAssert.SingleSequenceFieldItem(
            result.Values!,
            "Debug",
            valuesFieldValueSpan);
    }

    [Fact]
    public void Parse_MissingName_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestParsingContextFactory.Create(
                """
                type: string
                """,
                document);

        var nameFieldSpan = TestSourceSpan.Create(document, 1, 1, 2, 1);
        var typeFieldSpan = TestSourceSpan.Create(document, 1, 1, 1, 13);
        var typeFieldValueSpan = TestSourceSpan.Create(document, 1, 7, 1, 13);

        context.Cursor.StartDocument();

        var parameter = _parser.Parse(context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.ParameterNameIsRequired,
            nameFieldSpan);

        FieldAssert.IsStringKeyField(
            parameter.Name,
            "name",
            nameFieldSpan,
            string.Empty,
            nameFieldSpan);

        FieldAssert.IsStringKeyField(
            parameter.Type!,
            "type",
            typeFieldSpan,
            "string",
            typeFieldValueSpan);
    }

    private static string CreateRequiredNameField(
        string fieldName)
    {
        return fieldName is ParameterFieldNames.Name
            ? string.Empty
            : $"{ParameterFieldNames.Name}: configuration";
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
