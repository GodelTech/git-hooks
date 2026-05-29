using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Parameters;

public sealed class ParametersParserTests
{
    private readonly ParametersParser _parser = TestParserFactory.CreateParametersParser();

    [Fact]
    public void Constructor_NullParameterParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ParametersParser(null!));

        Assert.Equal(
            "parameterParser",
            exception.ParamName);
    }

    [Fact]
    public void Parse_EmptySequence_ReturnsEmptyCollection()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                "[]");

        cursor.StartDocument();

        var result = _parser.Parse(cursor);

        cursor.EndDocument();

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MultipleParameters_ReturnsParameters()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                - name: configuration
                  type: string

                - name: framework
                  type: string
                """);

        cursor.StartDocument();

        var result = _parser.Parse(cursor);

        cursor.EndDocument();

        Assert.Equal(
            2,
            result.Count);

        Assert.Equal(
            "configuration",
            result[0].Name);

        Assert.Equal(
            "framework",
            result[1].Name);
    }

    [Fact]
    public void Parse_NotSequence_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                name: configuration
                type: string
                """);

        cursor.StartDocument();

        var exception = Assert.Throws<YamlPipelineParsingException>(
            () => _parser.Parse(cursor));

        Assert.Contains(
            "Expected SequenceStart",
            exception.Message);
    }
}
