using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepsParserTests
{
    private readonly StepsParser _parser
        = TestParserFactory.CreateStepsParser();

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
        var context =
            TestParserFactory.CreateContext(
                "[]");

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MultipleSteps_ReturnsSteps()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                - script: dotnet test
                  displayName: Test

                - template: build.yml
                """);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        Assert.Collection(
            result,
            step => Assert.IsType<ScriptStepNode>(step),
            step => Assert.IsType<TemplateStepNode>(step));
    }

    [Fact]
    public void Parse_NotSequence_Throws()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                script: dotnet test
                displayName: Test
                """);

        context.Cursor.StartDocument();

        var exception = Assert.Throws<YamlException>(
            () => _parser.Parse(context));

        Assert.Contains(
            "Expected SequenceStart",
            exception.Message);
    }
}
