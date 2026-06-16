using System.Runtime.CompilerServices;

using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Expressions;

public sealed class ExpressionParserTests
{
    private readonly ExpressionParser _parser = TestParserFactory.CreateExpressionParser();

    [Fact]
    public void Constructor_NullInterpolatedStringParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ExpressionParser(null!));

        Assert.Equal(
            "interpolatedStringParser",
            exception.ParamName);
    }

    [Fact]
    public async Task Parse_PureInterpolation()
    {
        await VerifyAstAsync(
            "${{ parameters.project }}");
    }

    [Fact]
    public async Task Parse_AdjacentInterpolations()
    {
        await VerifyAstAsync(
            "${{ parameters.os }}${{ parameters.configuration }}");
    }

    [Fact]
    public async Task Parse_InterpolatedStringStartingWithExpression()
    {
        await VerifyAstAsync(
            "${{ parameters.project }}/bin");
    }

    [Fact]
    public async Task Parse_InterpolatedStringEndingWithExpression()
    {
        await VerifyAstAsync(
            "/src/${{ parameters.project }}");
    }

    [Fact]
    public async Task Parse_MultipleInterpolations()
    {
        await VerifyAstAsync(
            "${{ parameters.os }}-${{ parameters.configuration }}");
    }

    [Fact]
    public async Task Parse_InterpolationBetweenLiterals()
    {
        await VerifyAstAsync(
            "prefix-${{ parameters.name }}-suffix");
    }

    [Fact]
    public async Task Parse_BooleanLiteral()
    {
        await VerifyAstAsync("true");
    }

    [Fact]
    public async Task Parse_IntegerLiteral()
    {
        await VerifyAstAsync("123");
    }

    [Fact]
    public async Task Parse_StringLiteral()
    {
        await VerifyAstAsync("hello");
    }

    [Fact]
    public async Task Parse_StringWithEscapedCharacters()
    {
        await VerifyAstAsync(
            "echo \"Hello\"\nexit 0");
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
