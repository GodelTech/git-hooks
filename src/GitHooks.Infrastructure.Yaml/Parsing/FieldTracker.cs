using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class FieldTracker(
    FieldValueParser fieldValueParser)
{
    private readonly FieldValueParser _fieldValueParser
        = fieldValueParser ?? throw new ArgumentNullException(nameof(fieldValueParser));

    private readonly Dictionary<string, SourceSpan> _fields
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<FieldNode> _unknownFields
        = [];

    public T ReadFirst<T>(
        Scalar key,
        ParsingContext context,
        T currentValue,
        Func<ParsingContext, T> readValue)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(readValue);

        var fieldName = key.Value;

        var fieldSpan = context.Cursor.CreateSpan(
            key.Start,
            key.End);

        if (_fields.TryGetValue(fieldName, out var firstSpan))
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.DuplicateField,
                    fieldSpan,
                    [
                        DiagnosticLocation.Create(
                            firstSpan,
                            "First declaration is here.")
                    ],
                    fieldName));

            // Consume the duplicate value to keep parser state consistent.
            _ = readValue(context);

            return currentValue;
        }

        _fields.Add(
            fieldName,
            fieldSpan);

        return readValue(context);
    }

    public IReadOnlyList<FieldNode> GetUnknownFields()
    {
        return [.. _unknownFields];
    }

    public void AddUnknownField(
        Scalar key,
        ParsingContext context)
    {
        _unknownFields.Add(
            _fieldValueParser.ParseStringKeyField(
                key,
                context));
    }

    public ComplexKeyFieldNode<ValueNode> AddUnknownField(
        ParsingContext context)
    {
        var field = _fieldValueParser.ParseComplexKeyField(context);

        _unknownFields.Add(field);

        return field;
    }
}
