using GitHooks.Domain.Ast.Unknown;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class MappingFieldsTests
{
    private readonly MappingFields _fields =
        new(TestParserFactory.CreateUnknownNodeParser());

    [Fact]
    public void Constructor_NullUnknownNodeParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new MappingFields(null!));

        Assert.Equal(
            "unknownNodeParser",
            exception.ParamName);
    }

    [Fact]
    public void MarkSeen_DuplicateField_Throws()
    {
        var cursor = TestParserFactory.CreateDummyCursor();

        var key = new Scalar("steps");

        _fields.MarkSeen(
            key,
            cursor);

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => _fields.MarkSeen(
                    key,
                    cursor));

        Assert.StartsWith(
            "Duplicate 'steps' field",
            exception.Message);
    }

    [Fact]
    public void GetUnknownFields_InitiallyEmpty()
    {
        Assert.Empty(
            _fields.GetUnknownFields());
    }

    [Fact]
    public void AddUnknownField_AddsField()
    {
        var cursor =
            TestParserFactory.CreateCursor(
                "value");

        cursor.StartDocument();

        _fields.AddUnknownField(
            new Scalar("custom"),
            cursor);

        var fields = _fields.GetUnknownFields();

        var field = Assert.Single(fields);

        var simpleField = Assert.IsType<UnknownSimpleFieldNode>(field);

        Assert.Equal(
            "custom",
            simpleField.Key);
    }

    [Fact]
    public void AddUnknownField_WithComplexKey_AddsField()
    {
        var fields =
            new MappingFields(
                TestParserFactory.CreateUnknownNodeParser());

        var cursor =
            TestParserFactory.CreateCursor(
                """
                ? [1, 2]
                : value
                """);

        cursor.StartDocument();

        _ = cursor.Read<MappingStart>();

        fields.AddUnknownField(cursor);

        _ = cursor.Read<MappingEnd>();

        var field =
            Assert.Single(
                fields.GetUnknownFields());

        Assert.IsType<UnknownComplexFieldNode>(field);
    }
}
