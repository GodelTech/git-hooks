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
        var exception =
            Assert.Throws<ArgumentNullException>(
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
        var exception =
            Assert.Throws<ArgumentNullException>(
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
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseStringKeyField(
                    null!,
                    TestParsingContextFactory.CreateEmpty(
                        TestSourceDocument.Default)));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseStringKeyField_WithNullContext_Throws()
    {
        var key = TestScalar.Create("test");

        var exception =
            Assert.Throws<ArgumentNullException>(
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
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseSequenceField(
                    null!,
                    TestParsingContextFactory.CreateEmpty(
                        TestSourceDocument.Default)));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseSequenceField_WithNullContext_Throws()
    {
        var key = TestScalar.Create("values");

        var exception =
            Assert.Throws<ArgumentNullException>(
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
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseMappingField(
                    null!,
                    TestParsingContextFactory.CreateEmpty(
                        TestSourceDocument.Default)));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseMappingField_WithNullContext_Throws()
    {
        var key = TestScalar.Create("env");

        var exception =
            Assert.Throws<ArgumentNullException>(
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
            "${{ variables.configuration }}");
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

        var firstSpan = TestSourceSpan.Create(document, 1, 1, 1, 14);
        var secondSpan = TestSourceSpan.Create(document, 2, 1, 2, 14);

        var context =
            TestParsingContextFactory.Create(
                """
                configuration: Debug
                configuration: Release
                """,
                document);

        context.Cursor.StartDocument();

        var result =
            _parser.ParseMappingField(
                key,
                context);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "configuration");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            "First declaration is here.",
            firstSpan);

        var field = Assert.Single(result.Fields);

        AstAssert.HasStringField(
            field,
            "configuration",
            "Debug");
    }

    [Fact]
    public void ParseMappingField_WithComplexKey_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var expectedSpan = TestSourceSpan.Create(document, 1, 3, 2, 8);

        var context =
            TestParsingContextFactory.Create(
                """
                ? [1, 2]
                : value

                configuration: Release
                """,
                document);

        context.Cursor.StartDocument();

        var key = TestScalar.Create("mapping");

        var result =
            _parser.ParseMappingField(
                key,
                context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.MappingKeyMustBeScalar,
            expectedSpan,
            key.Value);

        var field = Assert.Single(result.Fields);

        AstAssert.HasStringField(
            field,
            "configuration",
            "Release");
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
