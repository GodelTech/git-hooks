using GitHooks.Domain.Ast;
using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

namespace GitHooks.Testing.Ast;

internal static class AstAssert
{
    public static TNode IsValidNode<TNode>(
        AstNode node,
        SourceSpan span)
        where TNode : AstNode
    {
        Assert.NotNull(node);

        var actualNode = Assert.IsType<TNode>(node);

        SourceSpanAssert.Equal(
            span,
            actualNode.Span);

        return actualNode;
    }
}
