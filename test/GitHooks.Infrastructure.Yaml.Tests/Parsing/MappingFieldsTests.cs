using GitHooks.Domain.Ast.Unknown;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;
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
        var context = TestParserFactory.CreateDummyContext();

        var key = new Scalar("steps");

        _fields.MarkSeen(
            key,
            context);

        var exception =
            Assert.Throws<YamlException>(
                () => _fields.MarkSeen(
                    key,
                    context));

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
        var context =
            TestParserFactory.CreateContext(
                "value");

        context.Cursor.StartDocument();

        _fields.AddUnknownField(
            new Scalar("custom"),
            context);

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

        var context =
            TestParserFactory.CreateContext(
                """
                ? [1, 2]
                : value
                """);

        context.Cursor.StartDocument();

        _ = context.Cursor.Read<MappingStart>();

        fields.AddUnknownField(context);

        _ = context.Cursor.Read<MappingEnd>();

        var field =
            Assert.Single(
                fields.GetUnknownFields());

        Assert.IsType<UnknownComplexFieldNode>(field);
    }
}
