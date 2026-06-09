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
        var cursor = TestParserFactory.CreateDummyCursor();

        var result = cursor.Read<StreamStart>();

        Assert.NotNull(result);
    }

    [Fact]
    public void Read_UnexpectedEvent_Throws()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        var exception =
            Assert.Throws<YamlException>(
                cursor.Read<MappingStart>);

        Assert.StartsWith(
            "Expected MappingStart",
            exception.Message);
    }

    [Fact]
    public void Is_CurrentEventMatches_ReturnsTrue()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        Assert.True(cursor.Is<StreamStart>());
    }

    [Fact]
    public void Is_CurrentEventDoesNotMatch_ReturnsFalse()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        Assert.False(cursor.Is<MappingStart>());
    }

    [Fact]
    public void CreateSpan_WithNullStart_Throws()
    {
        var (cursor, _, end) = CreateMappingCursor();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => cursor.CreateSpan(
                    null!, end));

        Assert.Equal(
            "start",
            exception.ParamName);
    }

    [Fact]
    public void CreateSpan_WithNullEnd_Throws()
    {
        var (cursor, start, _) = CreateMappingCursor();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => cursor.CreateSpan(
                    start, null!));

        Assert.Equal(
            "end",
            exception.ParamName);
    }

    [Fact]
    public void CreateSpan_WithEvents_ReturnsSpan()
    {
        var (cursor, start, end) = CreateMappingCursor();

        var expectedSpan =
            cursor.CreateSpan(
                start.Start,
                end.End);

        var result =
            cursor.CreateSpan(
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
        var cursor = TestParserFactory.CreateDummyCursor();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => cursor.CreateSpan(
                    null!));

        Assert.Equal(
            "exception",
            exception.ParamName);
    }

    [Fact]
    public void CreateSpan_WithYamlException_ReturnsSpan()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        var exception =
            new YamlException(
                new Mark(0, 1, 2),
                new Mark(0, 3, 4),
                "Test");

        var expected =
            cursor.CreateSpan(
                exception.Start,
                exception.End);

        var result =
            cursor.CreateSpan(
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

        var cursor = TestParserFactory.CreateDummyCursor();

        var result =
            cursor.CreateSpan(
                new Mark(1, 2, 3),
                new Mark(4, 5, 6));

        Assert.Equal(
            expectedSpan,
            result);
    }

    [Fact]
    public void CurrentSpan_AtEnd_ReturnsUnknown()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        _ = cursor.Read<StreamStart>();
        _ = cursor.Read<DocumentStart>();
        _ = cursor.Read<MappingStart>();
        _ = cursor.Read<MappingEnd>();
        _ = cursor.Read<DocumentEnd>();
        _ = cursor.Read<StreamEnd>();

        Assert.True(cursor.CurrentSpan().HasUnknownPosition);
    }

    [Fact]
    public void CreateException_ReturnsYamlException()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        var exception =
            cursor.CreateException(
                "Test");

        Assert.Equal(
            "Test",
            exception.Message);
    }

    private static (YamlParserCursor Cursor, MappingStart Start, MappingEnd End) CreateMappingCursor()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                """
                key: value
                """);

        _ = cursor.Read<StreamStart>();
        _ = cursor.Read<DocumentStart>();

        var start = cursor.Read<MappingStart>();

        _ = cursor.Read<Scalar>();
        _ = cursor.Read<Scalar>();

        var end = cursor.Read<MappingEnd>();

        return (cursor, start, end);
    }
}
