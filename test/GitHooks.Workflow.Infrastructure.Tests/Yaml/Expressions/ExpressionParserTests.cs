using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Expressions;

public class ExpressionParserTests
{
    private readonly ExpressionParser _parser = new();

    [Theory]
    [InlineData("eq(variables.Build, 'true')", "eq(variables.Build, 'true')")]
    [InlineData("  succeeded()  ", "succeeded()")]
    [InlineData("always()", "always()")]
    [InlineData("   ", "")]
    public void Parse_WithVariousInputs_ReturnsRawExpressionNodeWithTrimmedValue(
        string input,
        string expected)
    {
        // Arrange & Act
        var result = _parser.Parse(input);

        // Assert
        var node = Assert.IsType<RawExpressionNode>(result);

        Assert.Equal(expected, node.Value);
    }
}
