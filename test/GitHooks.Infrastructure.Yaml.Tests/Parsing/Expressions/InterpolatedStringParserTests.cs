using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Testing.Ast;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Expressions;

public sealed class InterpolatedStringParserTests
{
    private readonly InterpolatedStringParser _parser
        = TestParserFactory.CreateInterpolatedStringParser();

    [Fact]
    public void Constructor_NullVariableExpressionParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new InterpolatedStringParser(null!));

        Assert.Equal(
            "variableExpressionParser",
            exception.ParamName);
    }

    [Theory]
    [InlineData("${{ parameters.name }}", true)]
    [InlineData("${{parameters.name}}", true)]
    [InlineData("${{ parameters.name}}", true)]
    [InlineData("${{parameters.name }}", true)]
    [InlineData("${{  parameters.name  }}", true)]
    [InlineData("hello-${{ parameters.name }}", true)]
    [InlineData("${{ parameters.os }}-${{ parameters.configuration }}", true)]
    [InlineData("${{}}", true)]
    [InlineData("${{ }}", true)]
    [InlineData("${{    }}", true)]
    [InlineData("hello", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("${{", false)]
    [InlineData("}}", false)]
    [InlineData("${{ parameters.name", false)]
    [InlineData("parameters.name }}", false)]
    public void ContainsInterpolation_ReturnsExpectedResult(
        string value,
        bool expected)
    {
        var result = InterpolatedStringParser.ContainsInterpolation(value);

        Assert.Equal(
            expected,
            result);
    }

    [Fact]
    public void Parse_PureInterpolation_ReturnsVariable()
    {
        var result =
            _parser.Parse(
                "${{ parameters.project }}",
                SourceSpan.Unknown);

        var variable = Assert.IsType<VariableExpressionNode>(result);

        Assert.Equal(
            "parameters.project",
            variable.Path);
    }

    [Fact]
    public void Parse_InterpolatedString_ReturnsInterpolatedString()
    {
        var result =
            _parser.Parse(
                "/src/${{ parameters.project }}",
                SourceSpan.Unknown);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            2,
            interpolated.Parts.Count);

        AstAssert.HasStringValue(
            interpolated.Parts[0],
            "/src/");

        AstAssert.HasVariableValue(
            interpolated.Parts[1],
            "parameters.project");
    }

    [Fact]
    public void Parse_AdjacentInterpolations_ReturnsTwoVariables()
    {
        var result =
            _parser.Parse(
                "${{ parameters.os }}${{ parameters.configuration }}",
                SourceSpan.Unknown);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            2,
            interpolated.Parts.Count);

        AstAssert.HasVariableValue(
            interpolated.Parts[0],
            "parameters.os");

        AstAssert.HasVariableValue(
            interpolated.Parts[1],
            "parameters.configuration");
    }

    [Fact]
    public void Parse_MultipleInterpolations_ReturnsExpectedParts()
    {
        var result =
            _parser.Parse(
                "${{ parameters.os }}-${{ parameters.configuration }}",
                SourceSpan.Unknown);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            3,
            interpolated.Parts.Count);

        AstAssert.HasVariableValue(
            interpolated.Parts[0],
            "parameters.os");

        AstAssert.HasStringValue(
            interpolated.Parts[1],
            "-");

        AstAssert.HasVariableValue(
            interpolated.Parts[2],
            "parameters.configuration");
    }

    [Fact]
    public void Parse_InterpolationFollowedByLiteral_ReturnsExpectedParts()
    {
        var result =
            _parser.Parse(
                "${{ parameters.project }}/bin",
                SourceSpan.Unknown);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            2,
            interpolated.Parts.Count);

        AstAssert.HasVariableValue(
            interpolated.Parts[0],
            "parameters.project");

        AstAssert.HasStringValue(
            interpolated.Parts[1],
            "/bin");
    }
}
