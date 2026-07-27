using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Fields;

public sealed class FieldParserTests
{
    private readonly FieldParser _parser
        = TestParserFactory.CreateFieldParser();

    [Fact]
    public void Constructor_NullExpressionParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new FieldParser(
                null!,
                new FieldValueParser()));

        Assert.Equal(
            "expressionParser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullFieldValueParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new FieldParser(
                TestParserFactory.CreateExpressionParser(),
                null!));

        Assert.Equal(
            "fieldValueParser",
            exception.ParamName);
    }

    [Fact]
    public void ParseStringKeyField_WithNullKey_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => _parser.ParseStringKeyField(
                null!,
                TestYamlParserContextFactory.CreateEmpty(
                    TestSourceDocument.Default)));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseStringKeyField_WithNullContext_Throws()
    {
        var key = TestScalar.Create("test");

        var exception = Assert.Throws<ArgumentNullException>(
            () => _parser.ParseStringKeyField(
                key,
                null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void ParseSequenceField_WithNullKey_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => _parser.ParseSequenceField(
                null!,
                TestYamlParserContextFactory.CreateEmpty(
                    TestSourceDocument.Default)));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseSequenceField_WithNullContext_Throws()
    {
        var key = TestScalar.Create("values");

        var exception = Assert.Throws<ArgumentNullException>(
            () => _parser.ParseSequenceField(
                key,
                null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void ParseMappingField_WithNullKey_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => _parser.ParseMappingField(
                null!,
                TestYamlParserContextFactory.CreateEmpty(
                    TestSourceDocument.Default)));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseMappingField_WithNullContext_Throws()
    {
        var key = TestScalar.Create("env");

        var exception = Assert.Throws<ArgumentNullException>(
            () => _parser.ParseMappingField(
                key,
                null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public async Task ParseStringKeyField_WithStringValue()
    {
        await VerifyStringKeyFieldAsync(
            "Release");
    }

    [Fact]
    public async Task ParseStringKeyField_WithIntegerValue()
    {
        await VerifyStringKeyFieldAsync(
            "42");
    }

    [Fact]
    public async Task ParseStringKeyField_WithBooleanValue()
    {
        await VerifyStringKeyFieldAsync(
            "true");
    }

    [Fact]
    public async Task ParseStringKeyField_WithInterpolatedString()
    {
        await VerifyStringKeyFieldAsync(
            "${{ parameters.configuration }}");
    }

    [Fact]
    public async Task ParseSequenceField_WithMultipleItems()
    {
        await VerifySequenceFieldAsync(
            """
            - Release
            - Debug
            """);
    }

    [Fact]
    public async Task ParseSequenceField_WithMixedExpressions()
    {
        await VerifySequenceFieldAsync(
            """
            - Release
            - 42
            - true
            """);
    }

    [Fact]
    public async Task ParseMappingField_WithSingleField()
    {
        await VerifyMappingFieldAsync(
            """
            configuration: Release
            """);
    }

    [Fact]
    public async Task ParseMappingField_WithMultipleFields()
    {
        await VerifyMappingFieldAsync(
            """
            configuration: Release
            platform: x64
            """);
    }

    [Fact]
    public void ParseMappingField_WithDuplicateField_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var key = TestScalar.Create("parameters");

        var context =
            TestYamlParserContextFactory.Create(
                """
                configuration: Debug
                configuration: Release
                """,
                document);

        var firstKeySpan = TestSourceSpan.Create(document, 1, 1, 1, 14);
        var secondKeySpan = TestSourceSpan.Create(document, 2, 1, 2, 14);
        var configurationFieldSpan = TestSourceSpan.Create(document, 1, 1, 1, 21);
        var configurationFieldValueSpan = TestSourceSpan.Create(document, 1, 16, 1, 21);

        context.Cursor.StartDocument();

        var result =
            _parser.ParseMappingField(
                key,
                context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondKeySpan,
            "configuration");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstKeySpan);

        var field = Assert.Single(result.Fields);

        FieldAssert.IsStringKeyField(
            field,
            "configuration",
            configurationFieldSpan,
            "Debug",
            configurationFieldValueSpan);
    }

    [Fact]
    public void ParseMappingField_WithComplexKey_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                ? [1, 2]
                : value

                configuration: Release
                """,
                document);

        var complexKeySpan = TestSourceSpan.Create(document, 1, 3, 2, 8);
        var configurationFieldSpan = TestSourceSpan.Create(document, 4, 1, 4, 23);
        var configurationFieldValueSpan = TestSourceSpan.Create(document, 4, 16, 4, 23);

        context.Cursor.StartDocument();

        var key = TestScalar.Create("mapping");

        var result =
            _parser.ParseMappingField(
                key,
                context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MappingKeyMustBeScalar,
            complexKeySpan,
            key.Value);

        var field = Assert.Single(result.Fields);

        FieldAssert.IsStringKeyField(
            field,
            "configuration",
            configurationFieldSpan,
            "Release",
            configurationFieldValueSpan);
    }

    private async Task VerifyStringKeyFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        var key = TestScalar.Create("test");

        await ParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            context => _parser.ParseStringKeyField(
                key,
                context),
            memberName,
            sourceFilePath);
    }

    private async Task VerifySequenceFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        var key = TestScalar.Create("values");

        await ParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            context => _parser.ParseSequenceField(
                key,
                context),
            memberName,
            sourceFilePath);
    }

    private async Task VerifyMappingFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        var key = TestScalar.Create("mapping");

        await ParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            context => _parser.ParseMappingField(
                key,
                context),
            memberName,
            sourceFilePath);
    }
}
