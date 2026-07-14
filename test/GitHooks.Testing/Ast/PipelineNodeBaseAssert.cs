using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

internal static class PipelineNodeBaseAssert
{
    public static TNode IsValidPipelineNodeBase<TNode>(
        PipelineNodeBase pipelineNodeBase,
        SourceSpan span)
        where TNode : PipelineNodeBase
    {
        var actualPipelineNodeBase = AstAssert.IsValidNode<TNode>(
            pipelineNodeBase,
            span);

        Assert.Empty(actualPipelineNodeBase.UnknownFields);

        return actualPipelineNodeBase;
    }
}
