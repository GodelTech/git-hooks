using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class ValueAssert
{
    public static ScalarNode IsScalar(
        ValueNode valueNode,
        string value,
        SourceSpan span)
    {
        var actualValueNode = IsValidValue<ScalarNode>(
            valueNode,
            span);

        Assert.Equal(
            value,
            actualValueNode.Value);

        return actualValueNode;
    }

    public static SequenceNode IsSequenceWithItems(
        ValueNode valueNode,
        SourceSpan span)
    {
        var actualValueNode = IsValidValue<SequenceNode>(
            valueNode,
            span);

        Assert.NotEmpty(actualValueNode.Items);

        return actualValueNode;
    }

    private static TNode IsValidValue<TNode>(
        ValueNode value,
        SourceSpan span)
        where TNode : ValueNode
    {
        var actualValue = AstAssert.IsValidNode<TNode>(
            value,
            span);

        return actualValue;
    }
}
