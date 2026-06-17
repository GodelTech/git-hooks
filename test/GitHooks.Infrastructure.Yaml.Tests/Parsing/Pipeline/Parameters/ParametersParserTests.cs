using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;

using YamlDotNet.Core;

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
        var context =
            TestParserFactory.CreateContext(
                "[]");

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MultipleParameters_ReturnsParameters()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                - name: configuration
                  type: string

                - name: framework
                  type: string
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        Assert.Equal(
            2,
            result.Count);

        AstAssert.HasStringField(
            result[0].Name,
            "name",
            "configuration");

        AstAssert.HasStringField(
            result[1].Name,
            "name",
            "framework");
    }

    [Fact]
    public void Parse_NotSequence_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                name: configuration
                type: string
                """);

        context.Cursor.StartDocument();

        var exception = Assert.Throws<YamlException>(
            () => _parser.Parse(context));

        Assert.Contains(
            "Expected SequenceStart",
            exception.Message);
    }
}
