using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing.Unknown;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Unknown;

public sealed class UnknownNodeParserTests
{
    private readonly UnknownNodeParser _parser = TestParserFactory.CreateUnknownNodeParser();

    [Fact]
    public void ParseField_WithNullContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseField(
                    null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void ParseField_WithNullKey_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseField(
                    null!,
                    TestParserFactory.CreateDummyContext()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseField_WithKeyAndNullContext_Throws()
    {
        var key = new Scalar("test");

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseField(
                    key,
                    null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public async Task ParseField_WithSimpleValue()
    {
        await VerifySimpleFieldAsync(
            """
            custom:
              nested: value
            """);
    }

    [Fact]
    public async Task ParseField_WithScalarValue()
    {
        await VerifySimpleFieldAsync("value");
    }

    [Fact]
    public async Task ParseField_WithComplexKey()
    {
        await VerifyComplexFieldAsync(
            """
            ? [1, 2]
            : value
            """);
    }

    [Fact]
    public async Task ParseField_WithNestedComplexKey()
    {
        await VerifyComplexFieldAsync(
            """
            nested:
              ? [1, 2]
              : value
            """);
    }

    [Fact]
    public async Task ParseField_WithSequenceValue()
    {
        await VerifySimpleFieldAsync(
            """
            - one
            - two
            """);
    }

    [Fact]
    public async Task ParseField_WithMappingValue()
    {
        await VerifySimpleFieldAsync(
            """
            nested:
              child: value
            """);
    }

    [Fact]
    public void ParseField_WithUnsupportedNode_Throws()
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
                () => _parser.ParseField(context));

        Assert.StartsWith(
            "Unsupported unknown node",
            exception.Message);
    }

    private async Task VerifySimpleFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        var key = new Scalar("test");

        await TestParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            cursor => _parser.ParseField(key, cursor),
            memberName,
            sourceFilePath);
    }

    private async Task VerifyComplexFieldAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await TestParserSnapshotVerifier.VerifyAstAsync(
            yaml,
            _parser.ParseField,
            beforeParse: context => _ = context.Cursor.Read<MappingStart>(),
            afterParse: context => _ = context.Cursor.Read<MappingEnd>(),
            memberName,
            sourceFilePath);
    }
}
