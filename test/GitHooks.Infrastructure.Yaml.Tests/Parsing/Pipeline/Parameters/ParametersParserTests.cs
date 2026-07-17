using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Parameters;

public sealed class ParametersParserTests
{
    private readonly ParametersParser _parser
        = TestParserFactory.CreateParametersParser();

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
            TestYamlParserContextFactory.Create(
                "[]",
                TestSourceDocument.Default);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        DiagnosticAssert.Empty(context.Diagnostics);

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MultipleParameters_ReturnsParameters()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestYamlParserContextFactory.Create(
                """
                - name: configuration
                  type: string

                - name: framework
                  type: string
                """,
                document);

        var firstNameFieldSpan = TestSourceSpan.Create(document, 1, 3, 1, 22);
        var firstNameFieldValueSpan = TestSourceSpan.Create(document, 1, 9, 1, 22);
        var secondNameFieldSpan = TestSourceSpan.Create(document, 4, 3, 4, 18);
        var secondNameFieldValueSpan = TestSourceSpan.Create(document, 4, 9, 4, 18);

        context.Cursor.StartDocument();

        var result = _parser.Parse(context);

        context.Cursor.EndDocument();

        DiagnosticAssert.Empty(context.Diagnostics);

        Assert.Equal(
            2,
            result.Count);

        FieldAssert.IsStringKeyField(
            result[0].Name,
            "name",
            firstNameFieldSpan,
            "configuration",
            firstNameFieldValueSpan);

        FieldAssert.IsStringKeyField(
            result[1].Name,
            "name",
            secondNameFieldSpan,
            "framework",
            secondNameFieldValueSpan);
    }

    [Fact]
    public void Parse_NotSequence_Throws()
    {
        var context =
            TestYamlParserContextFactory.Create(
                """
                name: configuration
                type: string
                """,
                TestSourceDocument.Default);

        context.Cursor.StartDocument();

        var exception = Assert.Throws<YamlException>(
            () => _parser.Parse(context));

        Assert.Contains(
            "Expected SequenceStart",
            exception.Message);
    }
}
