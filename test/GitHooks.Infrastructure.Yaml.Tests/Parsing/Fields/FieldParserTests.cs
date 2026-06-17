using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Diagnostics;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Fields;

public sealed class FieldParserTests
{
    private readonly FieldParser _parser = TestParserFactory.CreateFieldParser();

    [Fact]
    public void ParseStringKeyField_WithNullKey_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseStringKeyField(
                    null!,
                    TestParserFactory.CreateDummyContext()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseStringKeyField_WithNullContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseStringKeyField(
                    new Scalar("test"),
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
                    TestParserFactory.CreateDummyContext()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseSequenceField_WithNullContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseSequenceField(
                    new Scalar("values"),
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
                    TestParserFactory.CreateDummyContext()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseMappingField_WithNullContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseMappingField(
                    new Scalar("env"),
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
        var context =
            TestParserFactory.CreateContext(
                """
                configuration: Debug
                configuration: Release
                """);

        context.Cursor.StartDocument();

        var result =
            _parser.ParseMappingField(
                new Scalar("parameters"),
                context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            "configuration");

        var field = Assert.Single(result.Fields);

        AstAssert.HasStringField(
            field,
            "configuration",
            "Debug");
    }

    [Fact]
    public async Task ParseMappingField_WithComplexKey()
    {
        await VerifyMappingFieldAsync(
            """
            ? [1, 2]
            : value

            configuration: Release
            """);
    }

    private async Task VerifyStringKeyFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        var key = new Scalar("test");

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
        var key = new Scalar("values");

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
        var key = new Scalar("mapping");

        await ParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            context => _parser.ParseMappingField(
                key,
                context),
            memberName,
            sourceFilePath);
    }
}
