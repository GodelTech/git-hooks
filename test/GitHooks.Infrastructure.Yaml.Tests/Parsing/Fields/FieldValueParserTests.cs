using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing.Fields;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Fields;

public sealed class FieldValueParserTests
{
    private readonly FieldValueParser _parser = TestParserFactory.CreateFieldValueParser();

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
        var key = new Scalar("test");

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
    public void ParseComplexKeyField_WithNullContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseComplexKeyField(
                    null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public async Task ParseStringKeyField_WithScalarValue()
    {
        await VerifyStringKeyFieldAsync(
            "value");
    }

    [Fact]
    public async Task ParseStringKeyField_WithSequenceValue()
    {
        await VerifyStringKeyFieldAsync(
            """
            - one
            - two
            """);
    }

    [Fact]
    public async Task ParseStringKeyField_WithMappingValue()
    {
        await VerifyStringKeyFieldAsync(
            """
            nested:
              child: value
            """);
    }

    [Fact]
    public async Task ParseComplexKeyField_WithScalarValue()
    {
        await VerifyComplexKeyFieldAsync(
            """
            ? [1, 2]
            : value
            """);
    }

    [Fact]
    public async Task ParseComplexKeyField_WithMappingValue()
    {
        await VerifyComplexKeyFieldAsync(
            """
            ? [1, 2]
            : 
              nested: value
            """);
    }

    [Fact]
    public async Task ParseComplexKeyField_WithNestedComplexKey()
    {
        await VerifyComplexKeyFieldAsync(
            """
            ? [1, 2]
            :
              ? [3, 4]
              : value
            """);
    }

    [Fact]
    public void ParseComplexKeyField_WithUnsupportedNode_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                test
                """);

        context.Cursor.StartDocument();

        _ = context.Cursor.Read<Scalar>();

        var exception =
            Assert.Throws<YamlException>(
                () => _parser.ParseComplexKeyField(context));

        Assert.StartsWith(
            "Expected scalar, sequence, or mapping.",
            exception.Message);
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

    private async Task VerifyComplexKeyFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await ParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            _parser.ParseComplexKeyField,
            beforeParse: context => _ = context.Cursor.Read<MappingStart>(),
            afterParse: context => _ = context.Cursor.Read<MappingEnd>(),
            memberName,
            sourceFilePath);
    }
}
