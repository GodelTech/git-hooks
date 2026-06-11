using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlParserCursorTests
{
    [Fact]
    public void Constructor_NullParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new YamlParserCursor(
                    null!,
                    new SourceDocument("test.yaml")));

        Assert.Equal(
            "parser",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_NullSourceDocument_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new YamlParserCursor(
                    new Parser(new StringReader("{}")),
                    null!));

        Assert.Equal(
            "sourceDocument",
            exception.ParamName);
    }

    [Fact]
    public void Create_NullYaml_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => YamlParserCursor.Create(
                    null!,
                    new SourceDocument("test.yaml")));

        Assert.Equal(
            "yaml",
            exception.ParamName);
    }

    [Fact]
    public void Create_EmptyYaml_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () => YamlParserCursor.Create(
                    string.Empty,
                    new SourceDocument("test.yaml")));

        Assert.Equal(
            "yaml",
            exception.ParamName);
    }

    [Fact]
    public void Create_NullSourceDocument_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => YamlParserCursor.Create(
                    "{}",
                    null!));

        Assert.Equal(
            "sourceDocument",
            exception.ParamName);
    }

    [Fact]
    public void Read_ExpectedEvent_ReturnsEvent()
    {
        var context = TestParserFactory.CreateDummyContext();

        var result = context.Cursor.Read<StreamStart>();

        Assert.NotNull(result);
    }

    [Fact]
    public void Read_UnexpectedEvent_Throws()
    {
        var context = TestParserFactory.CreateDummyContext();

        var exception =
            Assert.Throws<YamlException>(
                context.Cursor.Read<MappingStart>);

        Assert.StartsWith(
            "Expected MappingStart",
            exception.Message);
    }

    [Fact]
    public void Is_CurrentEventMatches_ReturnsTrue()
    {
        var context = TestParserFactory.CreateDummyContext();

        Assert.True(context.Cursor.Is<StreamStart>());
    }

    [Fact]
    public void Is_CurrentEventDoesNotMatch_ReturnsFalse()
    {
        var context = TestParserFactory.CreateDummyContext();

        Assert.False(context.Cursor.Is<MappingStart>());
    }

    [Fact]
    public void CreateSpan_WithNullStart_Throws()
    {
        var (context, _, end) = CreateMappingContext();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => context.Cursor.CreateSpan(
                    null!, end));

        Assert.Equal(
            "start",
            exception.ParamName);
    }

    [Fact]
    public void CreateSpan_WithNullEnd_Throws()
    {
        var (context, start, _) = CreateMappingContext();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => context.Cursor.CreateSpan(
                    start, null!));

        Assert.Equal(
            "end",
            exception.ParamName);
    }

    [Fact]
    public void CreateSpan_WithEvents_ReturnsSpan()
    {
        var (context, start, end) = CreateMappingContext();

        var expectedSpan =
            context.Cursor.CreateSpan(
                start.Start,
                end.End);

        var result =
            context.Cursor.CreateSpan(
                start,
                end);

        Assert.False(result.HasUnknownPosition);

        Assert.Equal(
            expectedSpan,
            result);
    }

    [Fact]
    public void CreateSpan_WithNullYamlException_Throws()
    {
        var context = TestParserFactory.CreateDummyContext();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => context.Cursor.CreateSpan(
                    null!));

        Assert.Equal(
            "exception",
            exception.ParamName);
    }

    [Fact]
    public void CreateSpan_WithYamlException_ReturnsSpan()
    {
        var context = TestParserFactory.CreateDummyContext();

        var exception =
            new YamlException(
                new Mark(0, 1, 2),
                new Mark(0, 3, 4),
                "Test");

        var expected =
            context.Cursor.CreateSpan(
                exception.Start,
                exception.End);

        var result =
            context.Cursor.CreateSpan(
                exception);

        Assert.Equal(
            expected,
            result);
    }

    [Fact]
    public void CreateSpan_WithMarks_ReturnsSpan()
    {
        var expectedSpan = new SourceSpan(
            new SourceDocument("test.yaml"),
            new SourcePosition(2, 3),
            new SourcePosition(5, 6));

        var context = TestParserFactory.CreateDummyContext();

        var result =
            context.Cursor.CreateSpan(
                new Mark(1, 2, 3),
                new Mark(4, 5, 6));

        Assert.Equal(
            expectedSpan,
            result);
    }

    [Fact]
    public void CurrentSpan_AtEnd_ReturnsUnknown()
    {
        var context = TestParserFactory.CreateDummyContext();

        _ = context.Cursor.Read<StreamStart>();
        _ = context.Cursor.Read<DocumentStart>();
        _ = context.Cursor.Read<MappingStart>();
        _ = context.Cursor.Read<MappingEnd>();
        _ = context.Cursor.Read<DocumentEnd>();
        _ = context.Cursor.Read<StreamEnd>();

        Assert.True(context.Cursor.CurrentSpan().HasUnknownPosition);
    }

    [Fact]
    public void CreateException_ReturnsYamlException()
    {
        var context = TestParserFactory.CreateDummyContext();

        var exception =
            context.Cursor.CreateException(
                "Test");

        Assert.Equal(
            "Test",
            exception.Message);
    }

    private static (ParsingContext Context, MappingStart Start, MappingEnd End) CreateMappingContext()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                key: value
                """);

        _ = context.Cursor.Read<StreamStart>();
        _ = context.Cursor.Read<DocumentStart>();

        var start = context.Cursor.Read<MappingStart>();

        _ = context.Cursor.Read<Scalar>();
        _ = context.Cursor.Read<Scalar>();

        var end = context.Cursor.Read<MappingEnd>();

        return (context, start, end);
    }
}
