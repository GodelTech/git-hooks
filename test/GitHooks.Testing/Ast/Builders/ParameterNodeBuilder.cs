using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast.Builders;

public sealed class ParameterNodeBuilder
    : MappingNodeBuilder<ParameterNodeBuilder>
{
    private readonly List<ExpressionNode> _values = [];

    private StringKeyFieldNode<ExpressionNode>? _name;
    private StringKeyFieldNode<ExpressionNode>? _displayName;
    private StringKeyFieldNode<ExpressionNode>? _type;
    private StringKeyFieldNode<ExpressionNode>? _defaultValue;

    public ParameterNodeBuilder()
    {
        WithName("configuration");
    }

    public ParameterNodeBuilder WithName(
        string name = "configuration")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _name = TestFields.StringKey(
            "name",
            name);

        return this;
    }

    public ParameterNodeBuilder WithDisplayName(
        string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        _displayName = TestFields.StringKey(
            "displayName",
            displayName);

        return this;
    }

    public ParameterNodeBuilder WithType(
        string type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        _type = TestFields.StringKey(
            "type",
            type);

        return this;
    }

    public ParameterNodeBuilder WithDefaultValue(
        string defaultValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultValue);

        _defaultValue = TestFields.StringKey(
            "defaultValue",
            defaultValue);

        return this;
    }

    public ParameterNodeBuilder WithValue(
        ExpressionNode value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _values.Add(value);

        return this;
    }

    public ParameterNodeBuilder WithValues(
        IEnumerable<ExpressionNode> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (var value in values)
        {
            WithValue(value);
        }

        return this;
    }

    public ParameterNodeBuilder WithValues(
        params string[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        WithValues(
            values.Select(TestExpressions.String));

        return this;
    }

    public ParameterNode Build()
    {
        if (_name is null)
        {
            throw new InvalidOperationException(
                "Name is required to build a ParameterNode.");
        }

        return new ParameterNode
        {
            Name = _name,
            DisplayName = _displayName,
            Type = _type,
            DefaultValue = _defaultValue,
            Values = _values.Count == 0
                ? null
                : new SequenceFieldNode<ExpressionNode>
                {
                    Key = "values",
                    Items = [.. _values],
                    Span = SourceSpan.Unknown
                },
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
