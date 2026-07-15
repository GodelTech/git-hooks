using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Testing.Ast.Builders.Expressions;
using GitHooks.Testing.Ast.Builders.Fields;

namespace GitHooks.Testing.Ast.Builders.Mappings.Parameters;

public sealed class ParameterNodeBuilder
    : PipelineNodeBuilderBase<ParameterNodeBuilder, ParameterNode>
{
    private StringKeyFieldNode<ExpressionNode>? _name;
    private StringKeyFieldNode<ExpressionNode>? _displayName;
    private StringKeyFieldNode<ExpressionNode>? _type;
    private StringKeyFieldNode<ExpressionNode>? _defaultValue;
    private SequenceFieldNode<ExpressionNode>? _values;

    public ParameterNodeBuilder WithName(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _name = Configure(configure).Build();

        return Self;
    }

    public ParameterNodeBuilder WithName(
        string name)
    {
        return WithName(x => x
            .WithKey("name")
            .WithStringValue(v => v.WithValue(name)));
    }

    public ParameterNodeBuilder WithDisplayName(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _displayName = Configure(configure).Build();

        return Self;
    }

    public ParameterNodeBuilder WithDisplayName(
        string displayName)
    {
        return WithDisplayName(x => x
            .WithKey("displayName")
            .WithStringValue(v => v.WithValue(displayName)));
    }

    public ParameterNodeBuilder WithType(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _type = Configure(configure).Build();

        return Self;
    }

    public ParameterNodeBuilder WithType(
        string type)
    {
        return WithType(x => x
            .WithKey("type")
            .WithStringValue(v => v.WithValue(type)));
    }

    public ParameterNodeBuilder WithDefaultValue(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _defaultValue = Configure(configure).Build();

        return Self;
    }

    public ParameterNodeBuilder WithDefaultValue(
        string defaultValue)
    {
        return WithDefaultValue(x => x
            .WithKey("defaultValue")
            .WithStringValue(v => v.WithValue(defaultValue)));
    }

    public ParameterNodeBuilder WithValues(
        Action<SequenceFieldNodeBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SequenceFieldNodeBuilder()
            .WithKey("values");

        if (_values is not null)
        {
            builder.WithItems(_values.Items);
        }

        configure.Invoke(builder);

        _values = builder.Build();

        return Self;
    }

    public ParameterNodeBuilder WithStringValue(
        Action<StringLiteralExpressionNodeBuilder> configure)
    {
        return WithValues(x => x
            .WithStringLiteralExpressionItem(configure));
    }

    public ParameterNodeBuilder WithValue(
        string value)
    {
        return WithStringValue(x => x
            .WithValue(value));
    }

    public override ParameterNode Build()
    {
        if (_name is null)
        {
            throw CreateRequiredPropertyException("Name");
        }

        return new ParameterNode
        {
            Name = _name,
            DisplayName = _displayName,
            Type = _type,
            DefaultValue = _defaultValue,
            Values = _values,
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
