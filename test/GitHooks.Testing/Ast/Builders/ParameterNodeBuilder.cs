using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Testing.Ast.Builders;

public sealed class ParameterNodeBuilder
    : MappingNodeBuilder<ParameterNodeBuilder>
{
    private readonly List<ExpressionNode> _values = [];

    private string? _name;
    private string? _displayName;
    private ParameterType _type = ParameterType.String;
    private ExpressionNode? _defaultValue;

    public ParameterNodeBuilder()
    {
        _name = "configuration";
    }

    public ParameterNodeBuilder WithName(
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _name = name;

        return this;
    }

    public ParameterNodeBuilder WithDisplayName(
        string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        _displayName = displayName;

        return this;
    }

    public ParameterNodeBuilder WithType(
        ParameterType type)
    {
        _type = type;

        return this;
    }

    public ParameterNodeBuilder WithDefaultValue(
        ExpressionNode defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue);

        _defaultValue = defaultValue;

        return this;
    }

    public ParameterNodeBuilder WithDefaultValue(
        string defaultValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultValue);

        WithDefaultValue(
            TestExpressions.String(defaultValue));

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
            throw new InvalidOperationException("Name is required to build a ParameterNode.");
        }

        return new ParameterNode
        {
            Name = _name,
            DisplayName = _displayName,
            Type = _type,
            DefaultValue = _defaultValue,
            Values = [.. _values],
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
