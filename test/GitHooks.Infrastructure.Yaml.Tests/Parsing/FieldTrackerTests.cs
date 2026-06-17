using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Diagnostics;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class FieldTrackerTests
{
    private readonly FieldTracker _fieldTracker = TestParserFactory.CreateFieldTracker();

    [Fact]
    public void Constructor_NullFieldValueParser_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new FieldTracker(null!));

        Assert.Equal(
            "fieldValueParser",
            exception.ParamName);
    }

    [Fact]
    public void ReadFirst_DuplicateField_ReportsDiagnosticAndKeepsCurrentValue()
    {
        var context = TestParserFactory.CreateDummyContext();

        var key = new Scalar("steps");

        var firstResult = _fieldTracker.ReadFirst(
            key,
            context,
            "current",
            static _ => "first");

        var secondResult = _fieldTracker.ReadFirst(
            key,
            context,
            firstResult,
            static _ => "second");

        Assert.Equal(
            "first",
            firstResult);

        Assert.Equal(
            "first",
            secondResult);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            "steps");
    }

    [Fact]
    public void GetUnknownFields_InitiallyEmpty()
    {
        Assert.Empty(
            _fieldTracker.GetUnknownFields());
    }

    [Fact]
    public void AddUnknownField_AddsField()
    {
        var context =
            TestParserFactory.CreateContext(
                "value");

        context.Cursor.StartDocument();

        _fieldTracker.AddUnknownField(
            new Scalar("custom"),
            context);

        var fields = _fieldTracker.GetUnknownFields();

        var field = Assert.Single(fields);

        var simpleField = Assert.IsType<StringKeyFieldNode<ValueNode>>(field);

        Assert.Equal(
            "custom",
            simpleField.Key);
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

        _fieldTracker.AddUnknownField(context);

        _ = context.Cursor.Read<MappingEnd>();

        var field =
            Assert.Single(
                _fieldTracker.GetUnknownFields());

        Assert.IsType<ComplexKeyFieldNode<ValueNode>>(field);
    }
}
