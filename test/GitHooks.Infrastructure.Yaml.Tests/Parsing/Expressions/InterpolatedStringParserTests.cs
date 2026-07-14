using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

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
    [InlineData("${{ variables.name }}", true)]
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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void Parse_EmptyString_Throws(
        string expression)
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var exception = Assert.Throws<ArgumentException>(
            () => _parser.Parse(expression, span, context));

        Assert.Equal(
            "value",
            exception.ParamName);
    }

    [Theory]
    [InlineData("${{ parameters.project }}", "project")]
    [InlineData("${{parameters.project}}", "project")]
    [InlineData("${{ parameters.project}}", "project")]
    [InlineData("${{parameters.project }}", "project")]
    [InlineData("${{  parameters.project  }}", "project")]
    public void Parse_PureInterpolation_ReturnsParameterVariable(
        string expression,
        string expected)
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                expression,
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        ExpressionAssert.IsParameterVariableExpression(
            result,
            expected,
            span);
    }

    [Theory]
    [InlineData("${{ foo.bar }}", "foo.bar")]
    [InlineData("${{ variables.configuration }}", "variables.configuration")]
    [InlineData("${{ env.prod }}", "env.prod")]
    public void Parse_UnsupportedVariableInterpolation_ReturnsInvalidVariableExpression(
        string expression,
        string expected)
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                expression,
                span,
                context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidVariableExpression,
            span,
            expected);

        ExpressionAssert.IsInvalidVariableExpression(
            result,
            expected,
            span);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("${{")]
    [InlineData("}}")]
    [InlineData("${{ parameters.name")]
    [InlineData("parameters.name }}")]
    public void Parse_NoInterpolation_ReturnsStringLiteralExpression(
        string expression)
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                expression,
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Single(interpolated.Parts);

        ExpressionAssert.IsStringLiteralExpression(
            interpolated.Parts[0],
            expression,
            span);
    }

    [Fact]
    public void Parse_InterpolatedString_ReturnsInterpolatedString()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                "/src/${{ parameters.project }}",
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            2,
            interpolated.Parts.Count);

        ExpressionAssert.IsStringLiteralExpression(
            interpolated.Parts[0],
            "/src/",
            span);

        ExpressionAssert.IsParameterVariableExpression(
            interpolated.Parts[1],
            "project",
            span);
    }

    [Fact]
    public void Parse_AdjacentInterpolations_ReturnsTwoParameterVariables()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                "${{ parameters.os }}${{ parameters.configuration }}",
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            2,
            interpolated.Parts.Count);

        ExpressionAssert.IsParameterVariableExpression(
            interpolated.Parts[0],
            "os",
            span);

        ExpressionAssert.IsParameterVariableExpression(
            interpolated.Parts[1],
            "configuration",
            span);
    }

    [Fact]
    public void Parse_MultipleInterpolations_ReturnsExpectedParts()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                "${{ parameters.os }}-${{ parameters.configuration }}",
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            3,
            interpolated.Parts.Count);

        ExpressionAssert.IsParameterVariableExpression(
            interpolated.Parts[0],
            "os",
            span);

        ExpressionAssert.IsStringLiteralExpression(
            interpolated.Parts[1],
            "-",
            span);

        ExpressionAssert.IsParameterVariableExpression(
            interpolated.Parts[2],
            "configuration",
            span);
    }

    [Fact]
    public void Parse_InterpolationFollowedByLiteral_ReturnsExpectedParts()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                "${{ parameters.project }}/bin",
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        var interpolated = Assert.IsType<InterpolatedStringExpressionNode>(result);

        Assert.Equal(
            2,
            interpolated.Parts.Count);

        ExpressionAssert.IsParameterVariableExpression(
            interpolated.Parts[0],
            "project",
            span);

        ExpressionAssert.IsStringLiteralExpression(
            interpolated.Parts[1],
            "/bin",
            span);
    }
}
