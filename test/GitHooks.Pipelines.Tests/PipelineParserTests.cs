using FluentAssertions;
using GitHooks.Pipelines.Models;

namespace GitHooks.Pipelines.Tests;

public partial class PipelineParserTests
{
    private static readonly PipelineParser _parser = new();

    [Fact]
    public void Parse_NullYaml_ReturnsError()
    {
        // Arrange
        string yaml = null;
        var expectedError = new ParseError("Input YAML cannot be null.", 0, 0);

        // Act
        var result = _parser.Parse(yaml);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        var resultError = Assert.Single(result.Errors);

        resultError
            .Should()
            .BeEquivalentTo(expectedError);
    }

    public static TheoryData<string, ParseError> MalformedYamlCases => new()
    {
        {
            """
            [unclosed
            """,
            new ParseError("While parsing a flow sequence, did not find expected ',' or ']'.", 2, 1)
        },
        {
            """
            {unclosed
            """,
            new ParseError("While parsing a flow mapping,  did not find expected ',' or '}'.", 2, 1)
        },
        {
            """
            - [invalid
            """,
            new ParseError("While parsing a flow sequence, did not find expected ',' or ']'.", 2, 1)
        }
    };

    [Theory]
    [MemberData(nameof(MalformedYamlCases))]
    public void Parse_MalformedYaml_ReturnsError(string yaml, ParseError expectedError)
    {
        // Arrange & Act
        var result = _parser.Parse(yaml);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        var resultError = Assert.Single(result.Errors);

        resultError
            .Should()
            .BeEquivalentTo(expectedError);
    }

    public static TheoryData<string, ParseError> UnknownKeyCases => new()
    {
        {
            """
            unknownKey: value
            """,
            new ParseError($"Property 'unknownKey' not found on type '{typeof(Pipeline)}'.", 1, 1)
        },
        {
            """
            steps: []
            unknownKey: value
            """,
            new ParseError($"Property 'unknownKey' not found on type '{typeof(Pipeline)}'.", 2, 1)
        }
    };

    [Theory]
    [MemberData(nameof(UnknownKeyCases))]
    public void Parse_UnknownKey_ReturnsError(string yaml, ParseError expectedError)
    {
        // Arrange & Act
        var result = _parser.Parse(yaml);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        var resultError = Assert.Single(result.Errors);

        resultError
            .Should()
            .BeEquivalentTo(expectedError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrEmptyYaml_ReturnsNullPipeline(string yaml)
    {
        // Arrange & Act
        var result = _parser.Parse(yaml);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Empty(result.Errors);
    }

    public static TheoryData<string, Pipeline> ValidYamlCases => new()
    {
        {
            "{}",
            new Pipeline()
        },
        {
            "steps: []",
            new Pipeline()
        },
        {
            "steps: null",
            new Pipeline
            {
                Steps = null
            }
        }
    };

    [Theory]
    [MemberData(nameof(ValidYamlCases))]
    [MemberData(nameof(StepsValidYamlCases))]
    public void Parse_ValidYaml_ReturnsExpectedPipeline(string yaml, Pipeline expected)
    {
        // Arrange & Act
        var result = _parser.Parse(yaml);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Errors);

        result.Value
            .Should()
            .BeEquivalentTo(expected);
    }
}
