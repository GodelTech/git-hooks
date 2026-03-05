using GitHooks.Pipelines.Models;

namespace GitHooks.Pipelines.Tests.Models;

public class ParseErrorTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var message = "Test message";
        var line = 5;
        var column = 10;

        // Act
        var error = new ParseError(message, line, column);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Equal(line, error.Line);
        Assert.Equal(column, error.Column);
    }

    [Fact]
    public void ToString_ReturnsFormattedMessage()
    {
        // Arrange
        var message = "Test message";
        var line = 5;
        var column = 10;
        var error = new ParseError(message, line, column);

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal($"({line},{column}): {message}", result);
    }
}
