using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing.Unknown;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Unknown;

public sealed class UnknownNodeParserTests
{
    private readonly UnknownNodeParser _parser = TestParserFactory.CreateUnknownNodeParser();

    [Fact]
    public void ParseField_WithNullCursor_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseField(
                    null!));

        Assert.Equal(
            "cursor",
            exception.ParamName);
    }

    [Fact]
    public void ParseField_WithNullKey_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseField(
                    null!,
                    TestParserFactory.CreateDummyCursor()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void ParseField_WithKeyAndNullCursor_Throws()
    {
        var key = new Scalar("test");

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.ParseField(
                    key,
                    null!));

        Assert.Equal(
            "cursor",
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
            ? [1, 2]
            :
              nested:
                - value
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
            beforeParse: cursor => _ = cursor.Read<MappingStart>(),
            afterParse: cursor => _ = cursor.Read<MappingEnd>(),
            memberName,
            sourceFilePath);
    }
}
