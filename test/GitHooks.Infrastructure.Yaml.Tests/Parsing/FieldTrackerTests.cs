using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class FieldTrackerTests
{
    private readonly FieldTracker _fieldTracker
        = TestParserFactory.CreateFieldTracker();

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
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var firstSpan = TestSourceSpan.Create(document, 1, 2, 3, 4);
        var secondSpan = TestSourceSpan.Create(document, 5, 6, 7, 8);

        var firstKey = TestScalar.Create("steps", firstSpan);
        var secondKey = TestScalar.Create("steps", secondSpan);

        var firstResult = _fieldTracker.ReadFirst(
            firstKey,
            context,
            "current",
            static _ => "first");

        var secondResult = _fieldTracker.ReadFirst(
            secondKey,
            context,
            firstResult,
            static _ => "second");

        Assert.Equal(
            "first",
            firstResult);

        Assert.Equal(
            "first",
            secondResult);

        var diagnostic = DiagnosticAssert.SingleWithRelatedLocations(
            context.Diagnostics,
            DiagnosticDescriptors.DuplicateField,
            secondSpan,
            "steps");

        DiagnosticAssert.SingleRelatedLocation(
            diagnostic,
            DiagnosticLocationMessages.PreviousDeclaration,
            firstSpan);
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
        var document = TestSourceDocument.Default;

        var key = TestScalar.Create("custom");

        var context =
            TestParsingContextFactory.Create(
                "value",
                document);

        var fieldSpan = TestSourceSpan.Create(document, 1, 1, 1, 6);
        var fieldValueSpan = TestSourceSpan.Create(document, 1, 1, 1, 6);

        context.Cursor.StartDocument();

        _fieldTracker.AddUnknownField(
            key,
            context);

        var fields = _fieldTracker.GetUnknownFields();

        var field = Assert.Single(fields);

        FieldAssert.IsStringKeyField(
            field,
            "custom",
            fieldSpan,
            "value",
            fieldValueSpan);
    }

    [Fact]
    public void AddUnknownField_WithComplexKey_AddsField()
    {
        var document = TestSourceDocument.Default;

        var context =
            TestParsingContextFactory.Create(
                """
                ? [1, 2]
                : value
                """,
                document);

        var fieldSpan = TestSourceSpan.Create(document, 1, 3, 2, 8);
        var fieldKeySpan = TestSourceSpan.Create(document, 1, 3, 1, 8);
        var fieldKeyFirstItemSpan = TestSourceSpan.Create(document, 1, 4, 1, 5);
        var fieldKeySecondItemSpan = TestSourceSpan.Create(document, 1, 7, 1, 8);
        var fieldValueSpan = TestSourceSpan.Create(document, 2, 3, 2, 8);

        context.Cursor.StartDocument();

        _ = context.Cursor.Read<MappingStart>();

        _fieldTracker.AddUnknownField(context);

        _ = context.Cursor.Read<MappingEnd>();

        var fields = _fieldTracker.GetUnknownFields();

        var field = Assert.Single(fields);

        var complexField = FieldAssert.IsComplexKeyField(
            field,
            fieldSpan,
            "value",
            fieldValueSpan);

        var complexFieldKey = ValueAssert.IsSequenceWithItems(
            complexField.Key,
            fieldKeySpan);

        Assert.Collection(
            complexFieldKey.Items,
            item => ValueAssert.IsScalar(item, "1", fieldKeyFirstItemSpan),
            item => ValueAssert.IsScalar(item, "2", fieldKeySecondItemSpan));
    }
}
