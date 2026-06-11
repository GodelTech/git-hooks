using GitHooks.Domain.Ast.Unknown;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class UnknownFieldTrackerTests
{
    private readonly UnknownFieldTracker _tracker =
        new(TestParserFactory.CreateUnknownNodeParser());

    [Fact]
    public void Constructor_NullUnknownNodeParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new UnknownFieldTracker(null!));

        Assert.Equal(
            "unknownNodeParser",
            exception.ParamName);
    }

    [Fact]
    public void GetUnknownFields_InitiallyEmpty()
    {
        Assert.Empty(
            _tracker.GetUnknownFields());
    }

    [Fact]
    public void AddUnknownField_WithScalarKey_AddsField()
    {
        var context = TestParserFactory.CreateContext("value");

        context.Cursor.StartDocument();

        _tracker.AddUnknownField(
            new Scalar("custom"),
            context);

        var field =
            Assert.IsType<UnknownSimpleFieldNode>(
                Assert.Single(
                    _tracker.GetUnknownFields()));

        Assert.Equal(
            "custom",
            field.Key);
    }

    [Fact]
    public void AddUnknownField_WithComplexKey_AddsField()
    {
        var context =
            TestParserFactory.CreateContext(
                """
                ? [1, 2]
                : value
                """);

        context.Cursor.StartDocument();

        _ = context.Cursor.Read<MappingStart>();

        _tracker.AddUnknownField(context);

        var field =
            Assert.Single(
                _tracker.GetUnknownFields());

        Assert.IsType<UnknownComplexFieldNode>(field);
    }
}
