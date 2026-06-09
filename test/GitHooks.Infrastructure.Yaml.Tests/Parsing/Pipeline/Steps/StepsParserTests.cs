using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepsParserTests
{
    private readonly StepsParser _parser = TestParserFactory.CreateStepsParser();

    [Fact]
    public void Constructor_NullStepParser_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new StepsParser(null!));

        Assert.Equal(
            "stepParser",
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
    public void Parse_MultipleSteps_ReturnsSteps()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                - script: dotnet test
                  displayName: Test

                - template: build.yml
                """);

        cursor.StartDocument();

        var result = _parser.Parse(cursor);

        cursor.EndDocument();

        Assert.Collection(
            result,
            step => Assert.IsType<ScriptStepNode>(step),
            step => Assert.IsType<TemplateStepNode>(step));
    }

    [Fact]
    public void Parse_NotSequence_Throws()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                script: dotnet test
                displayName: Test
                """);

        cursor.StartDocument();

        var exception = Assert.Throws<YamlException>(
            () => _parser.Parse(cursor));

        Assert.Contains(
            "Expected SequenceStart",
            exception.Message);
    }
}
