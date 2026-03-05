using GitHooks.Pipelines.Models;

namespace GitHooks.Pipelines.Tests.Models;

public class ParseResultTests
{
    [Fact]
    public void Ok_WithValue_ReturnsSuccessAndNoErrors()
    {
        // Arrange
        var pipeline = new Pipeline();

        // Act
        var result = ParseResult<Pipeline>.Ok(pipeline);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(pipeline, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Fail_WithErrors_ReturnsFailureAndNullValue()
    {
        // Arrange
        var errors = new[] { new ParseError("Test message", 1, 2) };

        // Act
        var result = ParseResult<Pipeline>.Fail(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Same(errors, result.Errors);
    }
}
