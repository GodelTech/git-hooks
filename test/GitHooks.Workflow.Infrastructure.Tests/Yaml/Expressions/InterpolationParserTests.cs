using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Expressions;

public class InterpolationParserTests
{
    private readonly InterpolationParser _parser = new();

    [Theory]
    [InlineData("hello world", "hello world")]
    [InlineData("  hello  ", "  hello  ")]
    [InlineData("", "")]
    public void Parse_WithVariousInputs_ReturnsNodeWithSameValue(string input, string expected)
    {
        // Arrange & Act
        var result = _parser.Parse(input);

        // Assert
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Parse_WithInterpolatedTemplate_ReturnsInterpolatedStringNodeWithSameValue()
    {
        // Arrange & Act
        var result = _parser.Parse("Hello, $(variables.Name)!");

        // Assert
        Assert.IsType<InterpolatedStringNode>(result);
        Assert.Equal("Hello, $(variables.Name)!", result.Value);
    }
}


