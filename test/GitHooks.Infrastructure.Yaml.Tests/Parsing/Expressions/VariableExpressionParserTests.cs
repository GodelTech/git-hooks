using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Expressions;

public sealed class VariableExpressionParserTests
{
    private readonly VariableExpressionParser _parser
        = TestParserFactory.CreateVariableExpressionParser();

    [Fact]
    public void Parse_ExpressionProvided_ReturnsVariableExpression()
    {
        var result =
            _parser.Parse(
                " parameters.configuration ",
                SourceSpan.Unknown);

        Assert.Equal(
            "parameters.configuration",
            result.Path);

        Assert.Equal(
            SourceSpan.Unknown,
            result.Span);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Parse_InvalidExpression_Throws(
        string expression)
    {
        Assert.Throws<ArgumentException>(
            () => _parser.Parse(
                expression,
                SourceSpan.Unknown));
    }
}
