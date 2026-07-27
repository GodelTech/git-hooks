using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Expressions;

public sealed class VariableExpressionParserTests
{
    private readonly VariableExpressionParser _parser
        = TestParserFactory.CreateVariableExpressionParser();

    [Fact]
    public void Parse_ExpressionProvided_ReturnsParameterVariableExpression()
    {
        var document = TestSourceDocument.Default;

        var context = TestYamlParserContextFactory.CreateEmpty(document);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var result =
            _parser.Parse(
                "parameters.configuration",
                span,
                context);

        DiagnosticAssert.Empty(context.Diagnostics);

        ExpressionAssert.IsParameterVariableExpression(
            result,
            "configuration",
            span);
    }

    [Theory]
    [InlineData(" parameters.configuration ")]
    [InlineData(" parameters.configuration")]
    [InlineData("parameters. configuration")]
    [InlineData("parameters.configuration ")]
    [InlineData("parameters. configuration ")]
    [InlineData("parameters.")]
    [InlineData("parameters. ")]
    public void Parse_ExpressionIsNotNormalized_ReturnsInvalidVariableExpression(
        string expression)
    {
        var document = TestSourceDocument.Default;

        var context = TestYamlParserContextFactory.CreateEmpty(document);

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
            expression);

        ExpressionAssert.IsInvalidVariableExpression(
            result,
            expression,
            span);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Parse_InvalidExpression_Throws(
        string expression)
    {
        var context = TestYamlParserContextFactory.CreateEmpty(TestSourceDocument.Default);

        var exception = Assert.Throws<ArgumentException>(
            () => _parser.Parse(
                expression,
                SourceSpan.Unknown,
                context));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }
}
