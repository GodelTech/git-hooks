using System.Globalization;

using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class FieldAssert
{
    public static StringKeyFieldNode<ExpressionNode> IsStringKeyField(
        StringKeyFieldNode<ExpressionNode> field,
        string key,
        SourceSpan span,
        Type valueType,
        string value,
        SourceSpan valueSpan)
    {
        var actualField = IsStringKeyField(
            field,
            key,
            span);

        if (valueType == typeof(int))
        {
            ExpressionAssert.IsIntegerLiteralExpression(
                actualField.Value,
                int.Parse(value, CultureInfo.InvariantCulture),
                valueSpan);
        }
        else if (valueType == typeof(string))
        {
            ExpressionAssert.IsStringLiteralExpression(
                actualField.Value,
                value,
                valueSpan);
        }
        else
        {
            throw new ArgumentException($"Unsupported value type: {valueType}", nameof(valueType));
        }

        return actualField;
    }

    public static StringKeyFieldNode<ExpressionNode> IsStringKeyField(
        StringKeyFieldNode<ExpressionNode> field,
        string key,
        SourceSpan span,
        string value,
        SourceSpan valueSpan)
    {
        var actualField = IsStringKeyField(
            field,
            key,
            span);

        ExpressionAssert.IsStringLiteralExpression(
            actualField.Value,
            value,
            valueSpan);

        return actualField;
    }

    public static StringKeyFieldNode<ValueNode> IsStringKeyField(
        FieldNode field,
        string key,
        SourceSpan span,
        string value,
        SourceSpan valueSpan)
    {
        var actualField = IsValidField<StringKeyFieldNode<ValueNode>>(
            field,
            span);

        Assert.Equal(
            key,
            actualField.Key);

        ValueAssert.IsScalar(
            actualField.Value,
            value,
            valueSpan);

        return actualField;
    }

    public static ComplexKeyFieldNode<ValueNode> IsComplexKeyField(
        FieldNode field,
        SourceSpan span,
        string value,
        SourceSpan valueSpan)
    {
        var actualField = IsValidField<ComplexKeyFieldNode<ValueNode>>(
            field,
            span);

        Assert.NotNull(actualField.Key);

        ValueAssert.IsScalar(
            actualField.Value,
            value,
            valueSpan);

        return actualField;
    }

    public static SequenceFieldNode<ExpressionNode> IsSequenceFieldWithItems(
        SequenceFieldNode<ExpressionNode> field,
        string key,
        SourceSpan span)
    {
        var actualField = IsValidField<SequenceFieldNode<ExpressionNode>>(
            field,
            span);

        Assert.Equal(
            key,
            actualField.Key);

        Assert.NotEmpty(actualField.Items);

        return actualField;
    }

    public static StringLiteralExpressionNode SingleSequenceFieldItem(
        SequenceFieldNode<ExpressionNode> field,
        string value,
        SourceSpan span)
    {
        Assert.NotNull(field);

        var item = Assert.Single(field.Items);

        return ExpressionAssert.IsStringLiteralExpression(
            item,
            value,
            span);
    }

    private static StringKeyFieldNode<ExpressionNode> IsStringKeyField(
        StringKeyFieldNode<ExpressionNode> field,
        string key,
        SourceSpan span)
    {
        var actualField = IsValidField<StringKeyFieldNode<ExpressionNode>>(
            field,
            span);

        Assert.Equal(
            key,
            actualField.Key);

        return actualField;
    }

    private static TNode IsValidField<TNode>(
        FieldNode field,
        SourceSpan span)
        where TNode : FieldNode
    {
        return AstAssert.IsValidNode<TNode>(
            field,
            span);
    }
}
